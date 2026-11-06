using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace MoreUpgrades;

/// <summary>
/// Makes the shop's upgrade stand taller by stacking copies of its two-row shelf box on top of it.
/// The game's own meshes are reused, so no asset bundle is needed.
///
/// ItemSetup runs ShopInitialize on every client once the level is ready. It finds the shelf slots right after this
/// prefix, so slots added here get filled like the originals. Slots only matter on the host; the shelf itself has
/// to be built on every client.
/// </summary>
internal static class ShelfPatches
{
    private const string Marker = "MoreUpgrades Shelf Extension";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShopManager), nameof(ShopManager.ShopInitialize), typeof(List<PunManager.ItemSpawn>))]
    private static void ExtendStands()
    {
        if (!SemiFunc.RunIsShop()) return;
        int levels = Plugin.ExtraShelfLevels.Value;
        ShelfLayout.Clear();

        // A failure here must not stop the shop from stocking.
        try
        {
            var stands = UnityEngine.Object.FindObjectsByType<UpgradeStand>(FindObjectsSortMode.None);
            if (stands.Length == 0) Plugin.Log.LogWarning("Shelf: no upgrade stand in this shop.");
            foreach (var stand in stands)
            {
                float height = levels > 0 ? Extend(stand, levels) : 0f;
                if (!SemiFunc.IsNotMasterClient()) ShelfLayout.Capture(stand, height);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Shelf: could not extend the upgrade stand: {e}");
        }
    }

    /// <returns>How much taller the stand got.</returns>
    private static float Extend(UpgradeStand stand, int levels)
    {
        Transform root = stand.transform;
        if (root.Find(Marker)) return 0f;

        MeshFilter shelf = stand.upgradeCompartment.GetComponentsInChildren<MeshFilter>(true)
            .FirstOrDefault(m => m.sharedMesh && m.sharedMesh.name.IndexOf("Shelf", StringComparison.OrdinalIgnoreCase) >= 0);
        Transform divider = root.Find("Cube (16)"), top = root.Find("Cube (10)"), back = root.Find("Cube (15)");
        Transform left = root.Find("Cube (7)"), right = root.Find("Cube (8)");
        if (!shelf || !divider || !top || !back || !left || !right)
        {
            Plugin.Log.LogWarning("Shelf: the upgrade stand's layout has changed, leaving it alone.");
            return 0f;
        }

        float step = shelf.sharedMesh.bounds.size.y * shelf.transform.lossyScale.y;
        var holder = new GameObject(Marker).transform;
        holder.SetParent(root, false);

        for (int level = 1; level <= levels; level++)
        {
            Vector3 lift = Vector3.up * (step * level);
            // Under the meshes transform so the copies shake with the machine when it rerolls.
            UnityEngine.Object.Instantiate(shelf.gameObject, shelf.transform.position + lift, shelf.transform.rotation,
                                           stand.allMeshesTransform);
            CopyCollider(divider, holder, divider.localPosition.y + step * level);
            CopyCollider(top, holder, top.localPosition.y + step * level);
        }

        // The stand's collision boxes are invisible; the back and sides run the full height of the new section.
        float height = step * levels;
        float middle = top.localPosition.y + height / 2f;
        CopyCollider(back, holder, middle, height);
        CopyCollider(left, holder, middle, height);
        CopyCollider(right, holder, middle, height);

        // Rerolling swaps whatever upgrades are inside this box (position + localScale), so stretch it over the new rows.
        Transform rerollBox = stand.upgradeInsideBoxCheck;
        rerollBox.position += Vector3.up * (height / 2f);
        rerollBox.localScale += Vector3.up * height;

        int slots = SemiFunc.IsNotMasterClient() ? 0 : AddSlots(root, step, levels);
        Plugin.Log.LogInfo($"Shelf: added {levels} level(s), {height:0.00}m taller, {slots} new upgrade slots" +
                           (SemiFunc.IsNotMasterClient() ? " (slots are handled by the host)." : "."));
        return height;
    }

    /// <summary>Slots sit in a shared list on the shop module, so this picks out the ones inside this stand's shelf.</summary>
    internal static bool IsStandSlot(Transform root, ItemVolume volume, float extraHeight)
    {
        if (volume.itemVolume != SemiFunc.itemVolume.upgrade) return false;
        if (volume.itemSecretShopType != SemiFunc.itemSecretShopType.none) return false;
        Vector3 local = root.InverseTransformPoint(volume.transform.position);
        return Mathf.Abs(local.x) <= 1f && local.y >= 0.9f && local.y <= 1.8f + extraHeight && local.z >= -0.2f && local.z <= 0.9f;
    }

    private static void CopyCollider(Transform source, Transform parent, float y, float height = -1f)
    {
        Transform copy = UnityEngine.Object.Instantiate(source, parent, false);
        Vector3 pos = source.localPosition;
        copy.localPosition = new Vector3(pos.x, y, pos.z);
        if (height > 0f)
        {
            Vector3 scale = source.localScale;
            copy.localScale = new Vector3(scale.x, height, scale.z);
        }
    }

    private static int AddSlots(Transform root, float step, int levels)
    {
        int added = 0;
        foreach (var volume in UnityEngine.Object.FindObjectsByType<ItemVolume>(FindObjectsSortMode.None))
        {
            if (!IsStandSlot(root, volume, 0f)) continue;

            Transform t = volume.transform;
            for (int level = 1; level <= levels; level++)
            {
                UnityEngine.Object.Instantiate(volume.gameObject, t.position + Vector3.up * (step * level), t.rotation, t.parent);
                added++;
            }
        }
        return added;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace MoreUpgrades;

/// <summary>
/// Puts the upgrades on the stand in color order, from the bottom row up, both when the shop is stocked and after a
/// reroll. All of this runs on the host, whose spawns are networked to everyone else.
/// </summary>
internal static class ShelfLayout
{
    private readonly struct Slot
    {
        public readonly ItemVolume Volume;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public Slot(ItemVolume volume)
        {
            Volume = volume;
            Position = volume.transform.position;
            Rotation = volume.transform.rotation;
        }
    }

    // The game destroys its slots after stocking, so rerolls use these saved positions.
    private static readonly Dictionary<UpgradeStand, List<List<Slot>>> standRows = new Dictionary<UpgradeStand, List<List<Slot>>>();

    // Card colors, read from the upgrade textures; the game has no field for them. Each entry matches item names.
    private static readonly string[][] ColorGroups =
    {
        new[] { "Grab Strength", "Grab Range", "Grab Throw" },       // orange
        new[] { "Energy", "Sprint Speed", "Crouch Rest" },           // yellow
        new[] { "Health" },                                          // green
        new[] { "Extra Jump", "Tumble" },                            // blue
        new[] { "Death Head Battery", "Map Player Count" },          // tan
    };

    private static int ColorRank(Item item)
    {
        for (int i = 0; i < ColorGroups.Length; i++)
            if (ColorGroups[i].Any(n => item.name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)) return i;
        return ColorGroups.Length; // an upgrade from a future patch or another mod goes last
    }

    internal static void Clear() => standRows.Clear();

    /// <summary>Saves the stand's slots as rows: bottom row first, each one left to right when facing the stand.</summary>
    internal static void Capture(UpgradeStand stand, float extraHeight)
    {
        Transform root = stand.transform;
        var slots = new List<(float x, float y, Slot slot)>();
        foreach (var volume in UnityEngine.Object.FindObjectsByType<ItemVolume>(FindObjectsSortMode.None))
        {
            if (!ShelfPatches.IsStandSlot(root, volume, extraHeight)) continue;
            Vector3 local = root.InverseTransformPoint(volume.transform.position);
            slots.Add((local.x, local.y, new Slot(volume)));
        }

        standRows[stand] = slots
            .GroupBy(s => Mathf.RoundToInt(s.y * 10f))
            .OrderBy(row => row.Key)
            .Select(row => row.OrderByDescending(s => s.x).Select(s => s.slot).ToList())
            .ToList();
    }

    /// <summary>
    /// Gives each color its own row when the rows can hold that, and packs the colors together otherwise. Anything
    /// past the last slot is returned in overflow.
    /// </summary>
    private static List<(Item item, Slot slot)> Plan(IEnumerable<Item> items, List<List<Slot>> rows, out List<Item> overflow)
    {
        var groups = items.GroupBy(ColorRank).OrderBy(g => g.Key)
            .Select(g => g.OrderBy(i => i.name, StringComparer.Ordinal).ToList()).ToList();
        int width = rows.Count > 0 ? rows.Max(r => r.Count) : 1;
        bool ownRows = groups.Sum(g => (g.Count + width - 1) / width) <= rows.Count;

        var placed = new List<(Item, Slot)>();
        overflow = new List<Item>();
        int row = 0, column = 0;
        foreach (var group in groups)
        {
            if (ownRows && column > 0) { row++; column = 0; }
            foreach (Item item in group)
            {
                while (row < rows.Count && column >= rows[row].Count) { row++; column = 0; }
                if (row < rows.Count) placed.Add((item, rows[row][column++]));
                else overflow.Add(item);
            }
        }
        return placed;
    }

    /// <summary>Called right before the shelves are filled. The fill takes upgrades from the end of the list.</summary>
    internal static void ArrangeForShop(ShopManager shop)
    {
        if (!Plugin.SortUpgradesByColor.Value || standRows.Count == 0) return;
        try
        {
            var rows = standRows.Values.SelectMany(r => r).ToList();
            var list = shop.potentialItemUpgrades;
            int count = Mathf.Min(shop.itemUpgradesAmount, list.Count);
            int start = list.Count - count;

            var placed = Plan(list.GetRange(start, count), rows, out var overflow);

            // First slot in the walk gets the last item in the list, so write the plan in reverse.
            list.RemoveRange(start, count);
            list.AddRange(overflow);
            for (int i = placed.Count - 1; i >= 0; i--) list.Add(placed[i].item);

            // Used stand slots go first, in plan order. Leftover stand slots go away so nothing else lands in them.
            var used = placed.Select(p => p.slot.Volume).ToList();
            var standSlots = new HashSet<ItemVolume>(rows.SelectMany(r => r).Select(s => s.Volume));
            foreach (ItemVolume volume in standSlots)
                if (!used.Contains(volume)) UnityEngine.Object.Destroy(volume.gameObject);

            var others = shop.itemVolumes.Where(v => !standSlots.Contains(v)).ToList();
            shop.itemVolumes.Clear();
            shop.itemVolumes.AddRange(used);
            shop.itemVolumes.AddRange(others);

            Plugin.Log.LogInfo($"Shelf: arranged {placed.Count} upgrades by color" +
                               (overflow.Count > 0 ? $", {overflow.Count} more go off the stand." : "."));
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Shelf: could not arrange upgrades by color: {e}");
        }
    }

    // Replaces the game's SpawnNewUpgrades: same picks and spawning, but new upgrades go in color order instead of
    // taking over the old upgrades' positions.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UpgradeStand), nameof(UpgradeStand.SpawnNewUpgrades))]
    private static bool RerollInColorOrder(UpgradeStand __instance)
    {
        if (!Plugin.SortUpgradesByColor.Value || !standRows.TryGetValue(__instance, out var rows)) return true;

        List<(Item item, Slot slot)> placed;
        List<Item> overflow;
        try
        {
            var cached = __instance.cachedUpgrades;
            var displayed = __instance.GetCurrentlyDisplayedUpgradeCounts(cached.Select(c => c.instanceID).ToList());
            var selected = new Dictionary<string, int>();
            var picks = new List<Item>();
            foreach (var old in cached)
            {
                Item pick = __instance.GetWeightedUpgradeExcluding(old.itemData, displayed, selected);
                if (!pick) continue;
                selected.TryAdd(pick.name, 0);
                selected[pick.name]++;
                picks.Add(pick);
            }
            placed = Plan(picks, rows, out overflow);
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Shelf: could not arrange the reroll by color: {e}");
            return true; // nothing has spawned yet, so the game's own version can still run
        }

        foreach (var (item, slot) in placed)
            Spawn(item, slot.Position, slot.Rotation * item.spawnRotationOffset);

        // Only possible if players stacked extra upgrades in the stand; those keep their old spots.
        for (int i = 0; i < overflow.Count; i++)
        {
            var old = __instance.cachedUpgrades[i];
            Spawn(overflow[i], old.position, old.rotation);
        }
        return false;
    }

    private static void Spawn(Item item, Vector3 position, Quaternion rotation)
    {
        if (SemiFunc.IsMultiplayer())
            PhotonNetwork.InstantiateRoomObject(item.prefab.ResourcePath, position, rotation, 0);
        else
            UnityEngine.Object.Instantiate(item.prefab.Prefab, position, rotation);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreUpgrades;

/// <summary>
/// The shop is rolled by the host only:
///   1. ShopManager.GetAllItemsFromStatsManager builds potentialItemUpgrades (one entry per copy still allowed,
///      unaffordable ones mostly skipped) and shuffles it.
///   2. PunManager.ShopPopulateItemVolumes walks the shelf slots ("item volumes") and fills upgrade-type slots
///      from the END of that list until itemUpgradesAmount (3) is reached.
/// We re-order the list so favored upgrades tend to sit at the end, raise the cap, and if the shop has fewer
/// upgrade slots than the new cap, turn a few spare small-item slots into upgrade slots.
/// </summary>
internal static class ShopPatches
{
    private const int VanillaUpgrades = 3;

    private static int Target => Mathf.RoundToInt(VanillaUpgrades * (1f + Plugin.ExtraPercent.Value / 100f));

    // Step 1: weighted shuffle (Efraimidis–Spirakis): key = u^(1/w); ascending sort puts heavy items last,
    // and SpawnShopItem takes from the last entry backwards.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShopManager), nameof(ShopManager.GetAllItemsFromStatsManager))]
    private static void BiasUpgrades(ShopManager __instance)
    {
        if (SemiFunc.IsNotMasterClient()) return;
        var list = __instance.potentialItemUpgrades;
        string[] favored = Plugin.Favored.Value
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        float w = Plugin.FavoredWeight.Value;
        if (list.Count < 2 || favored.Length == 0 || w <= 1f) return;

        bool IsFavored(Item it) => favored.Any(f => Matches(it, f));
        var sorted = list
            .Select(it => (it, key: Mathf.Pow(Random.Range(0.0001f, 1f), 1f / (IsFavored(it) ? w : 1f))))
            .OrderBy(p => p.key).Select(p => p.it).ToList();
        list.Clear();
        list.AddRange(sorted);
    }

    private static bool Matches(Item it, string f)
    {
        // Item asset names look like "Item Upgrade Player Grab Strength". Match on a word boundary so "Health"
        // doesn't need the full name, and "Strength" alone also works.
        string name = it.name ?? "";
        string display = it.itemName ?? "";
        return name.EndsWith(" " + f, StringComparison.OrdinalIgnoreCase)
            || name.IndexOf(" " + f + " ", StringComparison.OrdinalIgnoreCase) >= 0
            || display.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // Step 2: raise the cap and make sure there are enough upgrade slots, right before the shelves are filled.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PunManager), nameof(PunManager.ShopPopulateItemVolumes), typeof(List<PunManager.ItemSpawn>))]
    private static void MoreSlots()
    {
        if (SemiFunc.IsNotMasterClient()) return;
        var shop = ShopManager.instance;
        if (!shop) return;

        int target = Target;
        shop.itemUpgradesAmount = target;

        var volumes = shop.itemVolumes;
        int upgradeSlots = volumes.Count(v => v && v.itemVolume == SemiFunc.itemVolume.upgrade);
        int wanted = Mathf.Min(target, shop.potentialItemUpgrades.Count);
        int borrowed = 0;

        if (upgradeSlots < wanted)
        {
            // Borrow loose small-item slots (not ones on an item) — an upgrade fits on any small shelf spot.
            foreach (var v in volumes)
            {
                if (upgradeSlots + borrowed >= wanted) break;
                if (!v || v.itemVolume != SemiFunc.itemVolume.small) continue;
                if (v.GetComponentInParent<ItemAttributes>()) continue;
                v.itemVolume = SemiFunc.itemVolume.upgrade;
                borrowed++;
            }
        }

        Plugin.Log.LogInfo($"Shop: up to {target} upgrades (vanilla {VanillaUpgrades}), {shop.potentialItemUpgrades.Count} candidates, " +
                           $"{upgradeSlots} upgrade slots" + (borrowed > 0 ? $" + {borrowed} borrowed" : "") + ".");
    }
}

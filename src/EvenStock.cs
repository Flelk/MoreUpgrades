using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace MoreUpgrades;

/// <summary>Picks upgrades so every type gets about the same number of slots, with a few extra for Health.</summary>
internal static class EvenStock
{
    private static bool IsHealth(Item item) => item.name.IndexOf("Health", StringComparison.OrdinalIgnoreCase) >= 0;

    /// <summary>How many more of this upgrade the team can buy. Most upgrades have no limit.</summary>
    private static int CopiesLeft(Item item) =>
        item.maxPurchase ? item.maxPurchaseAmount - StatsManager.instance.GetItemsUpgradesPurchasedTotal(item.name) : int.MaxValue;

    internal static List<Item> Build(int count)
    {
        var left = StatsManager.instance.itemDictionary.Values
            .Where(i => i.itemType == SemiFunc.itemType.item_upgrade && CopiesLeft(i) > 0)
            .ToDictionary(i => i, CopiesLeft);
        var picks = new List<Item>();

        void Take(Item item)
        {
            picks.Add(item);
            left[item]--;
        }

        foreach (Item health in left.Keys.Where(IsHealth).ToList())
            for (int i = 0; i < Plugin.ExtraHealthUpgrades.Value && left[health] > 0 && picks.Count < count; i++)
                Take(health);

        // One of each per round, in a new order each time, so a shelf that doesn't divide evenly doesn't favor anyone.
        while (picks.Count < count)
        {
            var round = left.Where(p => p.Value > 0).Select(p => p.Key).OrderBy(_ => Random.value).ToList();
            if (round.Count == 0) break;
            foreach (Item item in round.Take(count - picks.Count)) Take(item);
        }
        return picks;
    }
}

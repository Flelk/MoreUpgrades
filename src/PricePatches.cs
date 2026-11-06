using System.Collections.Generic;
using HarmonyLib;

namespace MoreUpgrades;

/// <summary>
/// Gives every shop item one set price: the middle of its normal price range, with no increase for upgrades already
/// bought or levels completed. The game's discount for bigger lobbies still applies.
/// </summary>
internal static class PricePatches
{
    // The game's own values. ShopManager lives between shops, so these are needed if the setting is turned off.
    private static float? baseUpgrade, baseHealthPack, baseCrystal;

    // The increases are simple multipliers, so setting them to 0 also covers the game's affordability checks and rerolls.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShopManager), nameof(ShopManager.ShopInitialize), typeof(List<PunManager.ItemSpawn>))]
    private static void SetIncreases(ShopManager __instance)
    {
        baseUpgrade ??= __instance.upgradeValueIncrease;
        baseHealthPack ??= __instance.healthPackValueIncrease;
        baseCrystal ??= __instance.crystalValueIncrease;

        bool setPrices = Plugin.SetPrices.Value;
        __instance.upgradeValueIncrease = setPrices ? 0f : baseUpgrade.Value;
        __instance.healthPackValueIncrease = setPrices ? 0f : baseHealthPack.Value;
        __instance.crystalValueIncrease = setPrices ? 0f : baseCrystal.Value;
    }

    // GetValue rolls a random price between these two.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemAttributes), nameof(ItemAttributes.GetValue))]
    private static void RemovePriceRange(ItemAttributes __instance)
    {
        if (!Plugin.SetPrices.Value || !SemiFunc.RunIsShop()) return;
        __instance.itemValueMin = __instance.itemValueMax = (__instance.itemValueMin + __instance.itemValueMax) / 2f;
    }
}

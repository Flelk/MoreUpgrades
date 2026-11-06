using HarmonyLib;
using UnityEngine;

namespace MoreUpgrades;

/// <summary>
/// The shopkeeper's scan odds depend on how many discounts it has given in this shop: 100%, then 50%, then 33%, then
/// never. A failed scan doesn't change the odds, and items can be scanned as often as you like until then.
///
/// The game normally allows 3 scans at 1 in 3 each, and refuses more once itemDiscountAttempts reaches 3. We keep that
/// counter low until the third discount, then let the game's own refusal take over.
/// </summary>
internal static class DiscountPatches
{
    private const int DiscountLimit = 3;
    private static readonly float[] Odds = { 1f, 1f / 2f, 1f / 3f };

    private static ShopKeeper keeper;
    private static int discounts;

    private static int DiscountsGiven(ShopKeeper shopKeeper)
    {
        if (keeper != shopKeeper)
        {
            keeper = shopKeeper;
            discounts = 0;
        }
        return discounts;
    }

    private static int AttemptsFor(ShopKeeper shopKeeper) =>
        DiscountsGiven(shopKeeper) >= DiscountLimit ? ShopKeeper.itemDiscountAttemptLimit : 0;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShopKeeper), nameof(ShopKeeper.TryStartItemDiscount))]
    private static void KeepScansOpen(ShopKeeper __instance)
    {
        if (Plugin.DiscountOdds.Value && SemiFunc.IsMasterClientOrSingleplayer())
            __instance.itemDiscountAttempts = AttemptsFor(__instance);
    }

    // Replaces the game's 1 in 3 roll. Everything else matches ItemVerdictPositiveGet.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShopKeeper), nameof(ShopKeeper.ItemVerdictPositiveGet))]
    private static bool RollVerdict(ShopKeeper __instance, ref bool __result)
    {
        if (!Plugin.DiscountOdds.Value) return true;

        __result = false;
        if (!SemiFunc.IsMasterClientOrSingleplayer() || !__instance.ItemTargetValid()
            || !__instance.ItemDiscountTargetValid(__instance.itemReactObject)) return false;

        int given = DiscountsGiven(__instance);
        float odds = given < Odds.Length ? Odds[given] : 0f;
        __result = Random.value < odds;

        var reaction = ShopKeeperItemReaction.GetOrAdd(__instance.itemReactObject.gameObject);
        __instance.ItemShownSet(__instance.itemReactObject, reaction.TimesShownGet() + 1, !__result);
        Plugin.Log.LogInfo($"Shopkeeper: {odds:P0} scan {(__result ? "passed" : "failed")}, {given} discount(s) given before it.");
        return false;
    }

    // Counted here instead of at the verdict because the game skips this when the discount couldn't change the price.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShopKeeper), nameof(ShopKeeper.ItemDiscountLogic))]
    private static void CountDiscount(ShopKeeper __instance)
    {
        if (!Plugin.DiscountOdds.Value || !SemiFunc.IsMasterClientOrSingleplayer()) return;

        DiscountsGiven(__instance);
        discounts++;
        __instance.itemDiscountAttempts = AttemptsFor(__instance);
    }
}

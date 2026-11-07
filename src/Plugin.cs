using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace MoreUpgrades;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    internal static ConfigEntry<int> MinUpgrades;
    internal static ConfigEntry<int> MaxUpgrades;
    internal static ConfigEntry<bool> EvenUpgrades, FillUpgradeStand;
    internal static ConfigEntry<int> ExtraHealthUpgrades;
    internal static ConfigEntry<bool> GuaranteeFavored;
    internal static ConfigEntry<int> ExtraItemsMin;
    internal static ConfigEntry<int> ExtraItemsMax;
    internal static ConfigEntry<string> Favored;
    internal static ConfigEntry<float> FavoredWeight;
    internal static ConfigEntry<int> ExtraShelfLevels;
    internal static ConfigEntry<bool> SortUpgradesByColor;
    internal static ConfigEntry<bool> DiscountOdds;
    internal static ConfigEntry<bool> SetPrices;
    internal static ConfigEntry<bool> StrengthHotkeysEnabled;

    private void Awake()
    {
        Log = Logger;

        MinUpgrades = Config.Bind("Shop", "MinUpgrades", 10,
            new ConfigDescription("Each shop rolls a random number of upgrades from MinUpgrades to MaxUpgrades. " +
                                  "It can stock fewer if it runs out of room or available upgrades.",
                                  new AcceptableValueRange<int>(1, 50)));
        MaxUpgrades = Config.Bind("Shop", "MaxUpgrades", 25,
            new ConfigDescription("See MinUpgrades. Set both to the same number for a fixed amount.",
                                  new AcceptableValueRange<int>(1, 50)));
        ExtraItemsMin = Config.Bind("Shop", "ExtraItemsMin", 5,
            new ConfigDescription("Regular items, power crystals and health packs each get a random bonus from ExtraItemsMin to ExtraItemsMax. " +
                                  "The shop stocks less if it runs out of room or available items. 0 and 0 = game default.",
                                  new AcceptableValueRange<int>(0, 50)));
        ExtraItemsMax = Config.Bind("Shop", "ExtraItemsMax", 10,
            new ConfigDescription("See ExtraItemsMin.", new AcceptableValueRange<int>(0, 50)));
        GuaranteeFavored = Config.Bind("Shop", "GuaranteeFavored", true,
            "Stock one of each favored upgrade whenever the shop rolled it as available. " +
            "The game mostly skips upgrades the team can't afford, so a favored upgrade can still be missing.");
        EvenUpgrades = Config.Bind("Shop", "EvenUpgrades", true,
            "Give every upgrade type about the same number of slots. Replaces PreferredUpgrades and the game's per-type shop limits. Host only.");
        FillUpgradeStand = Config.Bind("Shop", "FillUpgradeStand", true,
            "Fill every slot on the upgrade stand. Replaces MinUpgrades and MaxUpgrades. Host only.");
        ExtraHealthUpgrades = Config.Bind("Shop", "ExtraHealthUpgrades", 2,
            new ConfigDescription("Extra Health upgrades on top of the even share, with EvenUpgrades on. Host only.", new AcceptableValueRange<int>(0, 20)));
        Favored = Config.Bind("Shop", "PreferredUpgrades", "Grab Strength, Grab Range, Health, Energy",
            "Comma-separated upgrades that show up more often (matched against the item name, e.g. " +
            "Grab Strength, Health, Energy (stamina), Sprint Speed, Extra Jump, Grab Range, Grab Throw, " +
            "Tumble Launch, Tumble Climb, Tumble Wings, Crouch Rest, Map Player Count, Death Head Battery). Empty = no bias.");
        FavoredWeight = Config.Bind("Shop", "PreferredWeight", 6f,
            new ConfigDescription("How much likelier a favored upgrade is to be picked than any other (1 = vanilla odds).",
                                  new AcceptableValueRange<float>(1f, 20f)));

        ExtraShelfLevels = Config.Bind("Shop", "ExtraShelfLevels", 2,
            new ConfigDescription("Levels stacked on the upgrade stand. Each adds two rows of 7 slots and about 0.85m; 2 roughly doubles its height. " +
                                  "0 = game default. Everyone in the lobby needs the same value, or upgrades look like they float.",
                                  new AcceptableValueRange<int>(0, 2)));

        SortUpgradesByColor = Config.Bind("Shop", "SortUpgradesByColor", true,
            "Arrange the upgrade stand by color from the bottom row up, including after a reroll. Only the host's setting matters.");

        DiscountOdds = Config.Bind("Shop", "ShopkeeperDiscountOdds", true,
            "Shopkeeper scans pass at 100%, then 50%, then 33% as it gives discounts, and never after the third. " +
            "Failed scans don't change the odds. False = game default. Only the host's setting matters.");
        SetPrices = Config.Bind("Shop", "SetPrices", true,
            "Every shop item has one set price that never goes up: the middle of its normal price range. " +
            "Bigger lobbies still get the game's discount. False = game default. Only the host's setting matters.");
        StrengthHotkeysEnabled = Config.Bind("Cheats", "StrengthHotkeys", true,
            "F8 adds 10 Grab Strength levels and F9 removes 10. Host or solo only. Changes are kept in your save.");

        var hotkeys = new GameObject("MoreUpgrades Hotkeys");
        hotkeys.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(hotkeys);
        hotkeys.AddComponent<StrengthHotkeys>();

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(ShopPatches));
        harmony.PatchAll(typeof(ShelfPatches));
        harmony.PatchAll(typeof(ShelfLayout));
        harmony.PatchAll(typeof(PricePatches));
        harmony.PatchAll(typeof(DiscountPatches));
        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
    }
}

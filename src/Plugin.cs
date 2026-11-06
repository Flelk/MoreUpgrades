using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace MoreUpgrades;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    internal static ConfigEntry<int> MinUpgrades;
    internal static ConfigEntry<int> MaxUpgrades;
    internal static ConfigEntry<bool> GuaranteeFavored;
    internal static ConfigEntry<int> ExtraItemsMin;
    internal static ConfigEntry<int> ExtraItemsMax;
    internal static ConfigEntry<string> Favored;
    internal static ConfigEntry<float> FavoredWeight;
    internal static ConfigEntry<int> ExtraShelfLevels;

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

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(ShopPatches));
        harmony.PatchAll(typeof(ShelfPatches));
        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
    }
}

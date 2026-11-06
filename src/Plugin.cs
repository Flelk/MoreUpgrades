using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace MoreUpgrades;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    internal static ConfigEntry<int> ExtraPercent;
    internal static ConfigEntry<string> Favored;
    internal static ConfigEntry<float> FavoredWeight;

    private void Awake()
    {
        Log = Logger;

        ExtraPercent = Config.Bind("Shop", "ExtraUpgradesPercent", 33,
            new ConfigDescription("How many more upgrade slots the shop gets, in percent of vanilla (3). " +
                                  "33 = 4 upgrades per shop. 100 = 6.", new AcceptableValueRange<int>(0, 300)));
        Favored = Config.Bind("Shop", "FavoredUpgrades", "Grab Strength, Health, Energy",
            "Comma-separated upgrades that show up more often (matched against the item name, e.g. " +
            "Grab Strength, Health, Energy (stamina), Sprint Speed, Extra Jump, Grab Range, Grab Throw, " +
            "Tumble Launch, Tumble Climb, Tumble Wings, Crouch Rest, Map Player Count, Death Head Battery). Empty = no bias.");
        FavoredWeight = Config.Bind("Shop", "FavoredWeight", 3f,
            new ConfigDescription("How much likelier a favored upgrade is to be picked than any other (1 = vanilla odds).",
                                  new AcceptableValueRange<float>(1f, 20f)));

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ShopPatches));
        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
    }
}

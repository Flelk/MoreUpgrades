using System.Collections.Generic;
using UnityEngine;

namespace MoreUpgrades;

/// <summary>
/// F8 and F9 change the local player's Grab Strength level through the game's own tester command. That command also
/// syncs to the lobby, but the game only accepts it from the host.
/// </summary>
internal class StrengthHotkeys : MonoBehaviour
{
    private const int Step = 10;

    private void Update()
    {
        if (!Plugin.StrengthHotkeysEnabled.Value) return;

        int change = Input.GetKeyDown(KeyCode.F8) ? Step : Input.GetKeyDown(KeyCode.F9) ? -Step : 0;
        if (change == 0 || !SemiFunc.NoTextInputsActive()) return;

        PlayerAvatar me = SemiFunc.PlayerAvatarLocal();
        if (!me || !me.physGrabber || !PunManager.instance) return;

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Plugin.Log.LogInfo("Strength hotkeys only work for the host.");
            return;
        }

        string id = SemiFunc.PlayerGetSteamID(me);
        if (SemiFunc.IsMultiplayer())
            PunManager.instance.photonView.RPC("TesterUpgradeCommandRPC", Photon.Pun.RpcTarget.All, id, "Strength", change);
        else
            PunManager.instance.TesterUpgradeCommandRPC(id, "Strength", change);

        int level = StatsManager.instance.playerUpgradeStrength.GetValueOrDefault(id, 0);
        Plugin.Log.LogInfo($"Grab Strength is now level {level}.");
        if (StatsUI.instance)
        {
            StatsUI.instance.Fetch();
            StatsUI.instance.ShowStats();
        }
    }
}

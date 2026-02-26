#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

using UnityEngine;

/// <summary>
/// Use to open the game overlay to store if steamworks is enabled.
/// If that fails, or steamworks is not enabled open the url.
/// </summary>
public class OpenSteamUpsell : MonoBehaviour
{
    private const uint SteamGameAppId = 0; // For example, DungeonSweeper appId = 4109840;
    private readonly string SteamGameUrl = "steam://store/" + SteamGameAppId.ToString();

    public void OpenSteamPage()
    {
        bool shouldUseSteam = false;
#if !DISABLESTEAMWORKS
        shouldUseSteam = SteamManager.Initialized;
#endif
        if (shouldUseSteam)
        {
#if !DISABLESTEAMWORKS
            try
            {
                SteamFriends.ActivateGameOverlayToStore(new AppId_t(SteamGameAppId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
            }
            catch
            {
                Application.OpenURL(SteamGameUrl);
            }
#endif
        }
        else
        {
            Application.OpenURL(SteamGameUrl);
        }
    }
}
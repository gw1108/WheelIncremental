#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

// This is a port of StatsAndAchievements.cpp from SpaceWar, the official Steamworks Example.
public class SteamStatsAndAchievements : MonoBehaviour
{
#if !DISABLESTEAMWORKS

    // Add an entry for each steam achievement.
    // Steam limits us to max 100 achievements. Commented out achivement rows are deleted to make room.
    /*
    private HashSet<AchievementSchema.Id> m_validSteamAchievements = new HashSet<AchievementSchema.Id> {
        AchievementSchema.Id.Adventurer0,
        AchievementSchema.Id.Adventurer1,
        AchievementSchema.Id.Adventurer2,
    };
    private Achievement_t[] m_Achievements = new Achievement_t[] {
        new Achievement_t(AchievementSchema.Id.Adventurer0.ToString(), "Novice Adventurer", "Complete the first dungeon level with the Adventurer."),
        new Achievement_t(AchievementSchema.Id.Adventurer1.ToString(), "Intermediate Adventurer", "Complete the second dungeon level with the Adventurer."),
        new Achievement_t(AchievementSchema.Id.Adventurer2.ToString(), "Advanced Adventurer", "Complete the third dungeon level with the Adventurer."),
        new Achievement_t(AchievementSchema.Id.Adventurer3.ToString(), "Master Adventurer", "Complete the fourth dungeon level with the Adventurer."),
        new Achievement_t(AchievementSchema.Id.BountyHunter0.ToString(), "Novice Bounty Hunter", "Complete the first dungeon level with the Bounty Hunter."),
        new Achievement_t(AchievementSchema.Id.BountyHunter1.ToString(), "Intermediate Bounty Hunter", "Complete the second dungeon level with the Bounty Hunter."),
        new Achievement_t(AchievementSchema.Id.BountyHunter2.ToString(), "Advanced Bounty Hunter", "Complete the third dungeon level with the Bounty Hunter."),
        new Achievement_t(AchievementSchema.Id.BountyHunter3.ToString(), "Master Bounty Hunter", "Complete the fourth dungeon level with the Bounty Hunter."),
        new Achievement_t(AchievementSchema.Id.BountyHunter4.ToString(), "Ascendant Bounty Hunter", "Complete the fifth dungeon level with the Bounty Hunter."),
    };
    */

    // Our GameID
    private CGameID m_GameID;

    // Did we get the stats from Steam?
    private bool m_bRequestedStats;
    private bool m_bStatsValid;

    // Should we store stats this frame?
    private bool m_bStoreStats;

    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    void OnEnable()
    {
        if (!SteamManager.Initialized)
            return;

        // Cache the GameID for use in the Callbacks
        m_GameID = new CGameID(SteamUtils.GetAppID());

        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        // These need to be reset to get the stats upon an Assembly reload in the Editor.
        m_bRequestedStats = false;
        m_bStatsValid = false;
    }

    private void Update()
    {
        if (!SteamManager.Initialized)
            return;

        if (!m_bRequestedStats)
        {
            // Is Steam Loaded? if no, can't get stats, done
            if (!SteamManager.Initialized)
            {
                m_bRequestedStats = true;
                return;
            }
        }

        if (!m_bStatsValid)
            return;

        // Get info from sources

        //Store stats in the Steam database if necessary
        if (m_bStoreStats)
        {
            // already set any achievements in UnlockAchievement

            bool bSuccess = SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try
            // again later.
            m_bStoreStats = !bSuccess;
        }
    }

    /// <summary>
    /// Unlock achievement based off id. Id is AchievementSchema.Id ToString().
    /// </summary>
    /// <param name="achievementID"></param>
    // TODO: Uncomment this when you have defined AchievementSchema.
    /*public void UnlockAchievement(AchievementSchema.Id achievementID)
    {
        string achievementIdString = achievementID.ToString();

        bool steamAchivementVersionExists = m_validSteamAchievements.Contains(achievementID);
        if (steamAchivementVersionExists)
        {
            Achievement_t achievement = m_Achievements.First(x => x.m_eAchievementID.Equals(achievementIdString, System.StringComparison.Ordinal));
            if (achievement != null)
            {
                UnlockAchievement(achievement);
                SteamUserStats.StoreStats();
            }
            else
            {
                Debug.LogWarning("Failed to find steam achievement with id: " + achievementID);
            }
        }
    }*/

    //-----------------------------------------------------------------------------
    // Purpose: Unlock this achievement
    //-----------------------------------------------------------------------------
    private void UnlockAchievement(Achievement_t achievement)
    {
        achievement.m_bAchieved = true;

        // the icon may change once it's unlocked
        //achievement.m_iIconImage = 0;

        // mark it down
        SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

        // Store stats end of frame
        m_bStoreStats = true;
    }

    //-----------------------------------------------------------------------------
    // Purpose: We have stats data from Steam. It is authoritative, so update
    //			our data with those results now.
    //-----------------------------------------------------------------------------
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!SteamManager.Initialized)
            return;

        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("Received stats and achievements from Steam\n");

                m_bStatsValid = true;

                // TODO iterate through all achievements and update ours with steam's version.
                // load achievements
                /*foreach (Achievement_t ach in m_Achievements)
                {
                    bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                    if (ret)
                    {
                        ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                        ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                    }
                    else
                    {
                        Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
                    }
                }*/
            }
            else
            {
                Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Our stats data was stored!
    //-----------------------------------------------------------------------------
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)m_GameID;
                OnUserStatsReceived(callback);
            }
            else
            {
                Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: An achievement was stored
    //-----------------------------------------------------------------------------
    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Display the user's stats and achievements
    //-----------------------------------------------------------------------------
    public void Render()
    {
        if (!SteamManager.Initialized)
        {
            GUILayout.Label("Steamworks not Initialized");
            return;
        }

        GUILayout.BeginArea(new Rect(Screen.width - 300, 0, 300, 800));
        // TODO: Optionally show all stats and achievements.
        /*foreach (Achievement_t ach in m_Achievements)
        {
            GUILayout.Label(ach.m_eAchievementID.ToString());
            GUILayout.Label(ach.m_strName + " - " + ach.m_strDescription);
            GUILayout.Label("Achieved: " + ach.m_bAchieved);
            GUILayout.Space(20);
        }*/

        // FOR TESTING PURPOSES ONLY!
        if (GUILayout.Button("RESET STATS AND ACHIEVEMENTS"))
        {
            SteamUserStats.ResetAllStats(true);
        }
        GUILayout.EndArea();
    }

    private class Achievement_t
    {
        public string m_eAchievementID;
        public string m_strName;
        public string m_strDescription;
        public bool m_bAchieved;

        /// <summary>
        /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
        /// </summary>
        /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
        /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
        /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
        public Achievement_t(string achievementID, string name, string desc)
        {
            m_eAchievementID = achievementID;
            m_strName = name;
            m_strDescription = desc;
            m_bAchieved = false;
        }
    }
#endif
}
using UnityEngine;

/// <summary>
/// This class acts as a ServiceLocator root and can be statically accessed via ServiceLocator.Instance.
/// This is what will eventually bootstrap the game scene.
/// </summary>
public class ServiceLocator : SingletonMonoBehaviour<ServiceLocator>
{
    // MonoBehavior backed systems
    // They must self-register
    //[HideInInspector]

    [HideInInspector]
    public SaveSystem SaveSystem;

    #region platformAndSteamRelatedStuff
    [HideInInspector]
    public SteamStatsAndAchievements SteamStatsAndAchievements;
    /// <summary>
    /// Set to true if this is steam demo.
    /// </summary>
    public static bool IsSteamDemo = false;
    /// <summary>
    /// Better to use SteamWorks.IsInitialized, but this returns true if you are compatible with Steam like UNITY_STANDALONE
    /// </summary>
    [Sirenix.OdinInspector.ReadOnly]
    public bool IsSteamCompatibleVersion = false;
    /// <summary>
    /// Determines if this application is iOS (iPhone).
    /// </summary>
    [Sirenix.OdinInspector.ReadOnly]
    public bool IsIOS = false;
    /// <summary>
    /// Determines if this application is android.
    /// </summary>
    [Sirenix.OdinInspector.ReadOnly]
    public bool IsAndroid = false;
    /// <summary>
    /// Determines if this application is webGL (browser game)
    /// </summary>
    [Sirenix.OdinInspector.ReadOnly]
    public bool IsWebGL = false;
    /// <summary>
    /// Determines if this application is linux.
    /// </summary>
    [Sirenix.OdinInspector.ReadOnly]
    public bool IsLinux = false;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);

            // 30FPS for the authentic dark souls experience
            QualitySettings.vSyncCount = 0;

            //AchievementSystem = new AchievementSystem();
            SaveSystem = new SaveSystem();

            string path = SaveSystem.GetSaveFilePath();

            FBPP.Start(new FBPPConfig()
            {
                SaveFileName = SaveSystem.saveFileName,
                AutoSaveData = true,
                ScrambleSaveData = true,
                EncryptionSecret = "WheelIncrementalSecret",
                SaveFilePath = path,
            });
        }

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
        IsSteamCompatibleVersion = true;
#endif
#if UNITY_IOS
        IsIOS = true;
#endif
#if UNITY_ANDROID
        IsAndroid = true;
#endif
#if UNITY_WEBGL
        IsWebGL = true;
#endif
#if UNITY_STANDALONE_LINUX
        IsLinux = true;
#endif
    }

    private void Start()
    {
        if (!IsSteamDemo && IsPaidVersion())
        {
            // Load the steam demo achievements that should be awarded and award them if you are not the demo and are pay to play steam version.
            //AchievementSystem.AwardSteamDemoAchievements();
        }
    }

    /// <summary>
    /// We assume that if you're playing on Steam or Android, it's a paid application.
    /// For Android, we should probably verify you downloaded it from Google Play Store....
    /// </summary>
    /// <returns></returns>
    public bool IsPaidVersion()
    {
        if (IsSteamDemo)
        {
            return false;
        }
        else
        {
            return SteamManager.Initialized || IsAndroid || Application.isEditor;
        }
    }

    public void Register(SteamStatsAndAchievements steamStatsAndAchievements)
    {
        SteamStatsAndAchievements = steamStatsAndAchievements;
    }
}

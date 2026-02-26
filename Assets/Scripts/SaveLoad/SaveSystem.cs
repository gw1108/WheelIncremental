#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System.IO;
using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SaveSystem
{
    public const string saveFileName = "WheelIncrementalSaveFile.txt";
    private string _saveFilePath;

    public static string GetSaveFilePath()
    {
        string path = Application.persistentDataPath;

#if PLATFORM_WEBGL
        path = "idbfs/WheelIncremental";
#endif

#if !DISABLESTEAMWORKS
        string steamPath;
        try
        {
            steamPath = SteamUser.GetSteamID().ToString(); // {64BitSteamID}
            path += "/" + steamPath;
        }
        catch
        {
            // Do nothing. Error usually caused by windows standalone not run from steam.
        }
#endif

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    public SaveSystem()
    {
        string path = GetSaveFilePath();
        Debug.Log("Saving player save/load to : " + path + " filename: player_data.json");
        _saveFilePath = Path.Combine(path, saveFileName);
    }

    /// <summary>
    /// Called when starting a dungeon level.
    /// </summary>
    public void SaveGame()
    {
        // Save something to FBPP
    }

    public void Wipe()
    {
        // Delete the FBPP save file. TODO
    }

    public bool HasSave()
    {
        // TODO
        return true;
    }
}
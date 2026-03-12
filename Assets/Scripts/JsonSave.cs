using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;

[System.Serializable]
public class PlayerData
{
    public int currentLevel;
    public int attempts;    
    public int points;
    public int misses;
    public int omissions;
    public int tips;
    public int differences;
    public float timeDifferences;
    public float time;
    public float accuracy;
}

public class JsonSave : MonoBehaviour
{
    public static JsonSave main;
    
    void Awake()
    {
        if (main == null)
        {
            main = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetPlayerData()
    {
        PlayerData playerData = JsonSave.LoadData<PlayerData>("playerData");

        playerData.currentLevel = 0;
        playerData.attempts = 0;
        playerData.points = 0;
        playerData.misses = 0;
        playerData.omissions = 0;
        playerData.tips = 0;
        playerData.differences = 0;
        playerData.timeDifferences = 0f;
        playerData.time = 0f;
        playerData.accuracy = 0f;

        SaveData(playerData, "PlayerData");
        PlayerPrefs.DeleteAll();
    }

    public static bool SaveData<T>(T data, string fileName)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(fileName);

            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static T LoadData<T>(string fileName) where T : new()
    {
        try
        {
            string path = GetSavePath(fileName);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }

            return new T();
        }
        catch (Exception e)
        {
            return new T();
        }
    }

    static string GetSavePath(string fileName)
    {
        if (!fileName.EndsWith(".json"))
        {
            fileName += ".json";
        }

        return Path.Combine(Application.persistentDataPath, fileName);
    }
}
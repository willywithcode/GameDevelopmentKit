namespace GameFoundation.Scripts.LocalData.Service
{
    using System;
    using Unity.Plastic.Newtonsoft.Json;
    using UnityEngine;

    public static class LocalDataUtils
    {
        public static void SaveData<T>(string key, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data for key: {key}. Error: {e.Message}");
            }
        }

        public static T LoadData<T>(string key) where T : new()
        {
            try
            {
                if (PlayerPrefs.HasKey(key))
                {
                    var json = PlayerPrefs.GetString(key);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load data for key: {key}. Error: {e.Message}");
            }

            return new();
        }

        public static void DeleteData(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }
    }
}
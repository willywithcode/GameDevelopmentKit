using GameFoundation.Scripts.LocalData.Interfaces;

namespace GameFoundation.Scripts.LocalData.Service
{
    using System;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using Newtonsoft.Json;
    using UnityEngine;

    public static class LocalDataUtils
    {
        private static readonly IGameLogger Logger = new LoggerService();

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
                Logger.Error($"Failed to save data for key: {key}. Error: {e.Message}");
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
                Logger.Error($"Failed to load data for key: {key}. Error: {e.Message}");
            }
            var data = new T();
            ((ILocalData)data).Reset();
            return data;
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

namespace GameFoundation.Scripts.LocalData.Service
{
    using System;
    using GameFoundation.Scripts.LocalData.Interfaces;
    using UnityEngine;

    public abstract class BaseLocalDataService<T> : ILocalDataService<T> where T : ILocalData, new()
    {
        public T Data { get; private set; }

        protected BaseLocalDataService()
        {
            this.Data = new();
            this.Load();
        }

        public virtual void Save()
        {
            try
            {
                var key = this.Data.GetKey();
                LocalDataUtils.SaveData(key, this.Data);
                Debug.Log($"Saved data with key: {key}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving data: {e.Message}");
            }
        }

        public virtual void Load()
        {
            try
            {
                var key = this.Data.GetKey();
                this.Data = LocalDataUtils.LoadData<T>(key);
                Debug.Log($"Loaded data with key: {key}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading data: {e.Message}");
                this.Data ??= new();
                this.Data.Reset();
            }
        }

        public virtual void DeleteData()
        {
            try
            {
                var key = this.Data.GetKey();
                LocalDataUtils.DeleteData(key);
                this.Data.Reset();
                Debug.Log($"Deleted data with key: {key}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting data: {e.Message}");
            }
        }
    }
}
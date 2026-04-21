namespace GameFoundation.Scripts.LocalData.Service
{
    using System;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.LocalData.Interfaces;

    public abstract class BaseLocalDataService<T> : ILocalDataService<T> where T : ILocalData, new()
    {
        private readonly IGameLogger logger;
        public T Data { get; private set; }

        protected BaseLocalDataService(IGameLogger logger = null)
        {
            this.logger = logger ?? new LoggerService();
            this.Data = new();
            this.Load();
        }

        public virtual void Save()
        {
            try
            {
                var key = this.Data.GetKey();
                LocalDataUtils.SaveData(key, this.Data);
                this.logger.Info($"Saved data with key: {key}");
            }
            catch (Exception e)
            {
                this.logger.Error($"Error saving data: {e.Message}");
            }
        }

        public virtual void Load()
        {
            try
            {
                var key = this.Data.GetKey();
                this.Data = LocalDataUtils.LoadData<T>(key);
                this.logger.Info($"Loaded data with key: {key}");
            }
            catch (Exception e)
            {
                this.logger.Error($"Error loading data: {e.Message}");
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
                this.logger.Info($"Deleted data with key: {key}");
            }
            catch (Exception e)
            {
                this.logger.Error($"Error deleting data: {e.Message}");
            }
        }
    }
}

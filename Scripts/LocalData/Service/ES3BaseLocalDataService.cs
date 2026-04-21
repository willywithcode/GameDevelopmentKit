namespace GameFoundation.Scripts.LocalData.Service
{
    using System;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.LocalData.Interfaces;

    public abstract class ES3BaseLocalDataService<T> : ILocalDataService<T> where T : ILocalData, new()
    {
        private readonly IGameLogger logger;
        public T Data { get; private set; }

        protected ES3BaseLocalDataService(IGameLogger logger = null)
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
                ES3.Save(key, this.Data);
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
                this.Data = ES3.KeyExists(key) ? ES3.Load<T>(key) : new T();
                if (!ES3.KeyExists(key)) this.Data.Reset();
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
                ES3.DeleteKey(key);
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

namespace GameFoundation.Scripts.LocalData.Service
{
    using System;
    using CodeStage.AntiCheat.Storage;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.LocalData.Interfaces;
    using Newtonsoft.Json;

    /// <summary>
    /// Encrypted local data service backed by ObscuredPrefs (ACTk).
    /// Data is AES-encrypted, device-locked, and tamper-detected automatically.
    /// Subscribe to OnTamperDetected / OnForeignDeviceDetected to react to attacks.
    /// </summary>
    public abstract class ACTkBaseLocalDataService<T> : ILocalDataService<T> where T : ILocalData, new()
    {
        private readonly IGameLogger logger;
        public T Data { get; private set; }

        /// <summary>Fired when save data has been tampered with.</summary>
        public event Action OnTamperDetected;

        /// <summary>Fired when save data was written on a different device.</summary>
        public event Action OnForeignDeviceDetected;

        protected ACTkBaseLocalDataService(IGameLogger logger = null)
        {
            ObscuredPrefs.NotGenuineDataDetected        += this.HandleTamper;
            ObscuredPrefs.DataFromAnotherDeviceDetected += this.HandleForeignDevice;

            this.logger = logger ?? new LoggerService();
            this.Data = new T();
            this.Load();
        }

        public virtual void Save()
        {
            try
            {
                var key  = this.Data.GetKey();
                var json = JsonConvert.SerializeObject(this.Data);
                ObscuredPrefs.SetString(key, json);
                ObscuredPrefs.Save();
                this.logger.Info($"[ACTkLocalData] Saved: {key}");
            }
            catch (Exception e)
            {
                this.logger.Error($"[ACTkLocalData] Error saving: {e.Message}");
            }
        }

        public virtual void Load()
        {
            try
            {
                var key  = this.Data.GetKey();
                var json = ObscuredPrefs.GetString(key, string.Empty);

                this.Data = !string.IsNullOrEmpty(json)
                    ? JsonConvert.DeserializeObject<T>(json) ?? new T()
                    : new T();

                if (string.IsNullOrEmpty(json)) this.Data.Reset();

                this.logger.Info($"[ACTkLocalData] Loaded: {key}");
            }
            catch (Exception e)
            {
                this.logger.Error($"[ACTkLocalData] Error loading: {e.Message}");
                this.Data ??= new T();
                this.Data.Reset();
            }
        }

        public virtual void DeleteData()
        {
            try
            {
                var key = this.Data.GetKey();
                ObscuredPrefs.DeleteKey(key);
                ObscuredPrefs.Save();
                this.Data.Reset();
                this.logger.Info($"[ACTkLocalData] Deleted: {key}");
            }
            catch (Exception e)
            {
                this.logger.Error($"[ACTkLocalData] Error deleting: {e.Message}");
            }
        }

        private void HandleTamper()
        {
            this.logger.Warning($"[ACTkLocalData] Tamper detected on: {this.Data.GetKey()}");
            this.Data.Reset();
            this.OnTamperDetected?.Invoke();
        }

        private void HandleForeignDevice()
        {
            this.logger.Warning($"[ACTkLocalData] Foreign device save detected on: {this.Data.GetKey()}");
            this.OnForeignDeviceDetected?.Invoke();
        }
    }
}

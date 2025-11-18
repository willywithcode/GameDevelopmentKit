// File: AssetsManager.cs

namespace GameFoundation.Scripts.Addressable
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Networking;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using Object = UnityEngine.Object;

    public class AssetsManager : IAssetsManager
    {
        // Asset cache: key -> (asset, handle, refCount)
        private readonly ConcurrentDictionary<string, (Object asset, AsyncOperationHandle handle, int refCount)> assetCache = new();

        // Download handles: id -> handle
        private readonly ConcurrentDictionary<Guid, AsyncOperationHandle> downloadHandles = new();

        // Events
        public event Action<string, Object>    OnAssetLoaded;
        public event Action<string, Exception> OnAssetLoadFailed;
        public event Action                    OnCatalogUpdated;

        // Init state
        private          bool   isInitialized;
        private readonly object initGate = new();

        // ------------- Initialization -------------
        private async UniTask EnsureInitialized()
        {
            if (this.isInitialized) return;
            lock (this.initGate)
                if (this.isInitialized)
                    return;
            var initHandle = Addressables.InitializeAsync(autoReleaseHandle: false);
            await initHandle.Task.AsUniTask();
            if (initHandle.Status == AsyncOperationStatus.Failed) {
                Debug.LogError($"Addressables initialization failed: {initHandle.OperationException?.Message}");
                return;
            }
            lock (this.initGate) this.isInitialized = true;
        }

        // ------------- Load APIs -------------
        public T LoadAsset<T>(string key) where T : Object
        {
            if (string.IsNullOrEmpty(key)) {
                var ex = new ArgumentException("Asset key is null or empty.");
                this.OnAssetLoadFailed?.Invoke(key, ex);
                throw ex;
            }
            if (this.assetCache.TryGetValue(key, out var existed) && existed.asset is T tCached) {
                this.assetCache.AddOrUpdate(key, existed, (k, v) => (v.asset, v.handle, v.refCount + 1));
                this.OnAssetLoaded?.Invoke(key, tCached);
                return tCached;
            }
            this.EnsureInitialized().Forget();
            var handle = Addressables.LoadAssetAsync<T>(key);
            var asset  = handle.WaitForCompletion();
            if (handle.Status != AsyncOperationStatus.Succeeded) {
                var ex = new InvalidOperationException($"Load failed for key '{key}': {handle.OperationException?.Message}");
                this.OnAssetLoadFailed?.Invoke(key, ex);
                Addressables.Release(handle);
                throw ex;
            }
            this.assetCache[key] = (asset, handle, 1);
            this.OnAssetLoaded?.Invoke(key, asset);
            return asset;
        }

        public async UniTask<T> LoadAssetAsync<T>(string key, IProgress<float> progress = null) where T : Object
        {
            if (string.IsNullOrEmpty(key)) {
                var ex = new ArgumentException("Asset key is null or empty.");
                this.OnAssetLoadFailed?.Invoke(key, ex);
                throw ex;
            }
            if (this.assetCache.TryGetValue(key, out var existed) && existed.asset is T tCached) {
                this.assetCache.AddOrUpdate(key, existed, (k, v) => (v.asset, v.handle, v.refCount + 1));
                progress?.Report(1f);
                this.OnAssetLoaded?.Invoke(key, tCached);
                return tCached;
            }
            // await this.EnsureInitialized();
            var handle = Addressables.LoadAssetAsync<T>(key);
            while (!handle.IsDone) {
                progress?.Report(handle.PercentComplete);
                await UniTask.Yield();
            }
            if (handle.Status != AsyncOperationStatus.Succeeded) {
                var ex = new InvalidOperationException($"Load failed for key '{key}': {handle.OperationException?.Message}");
                this.OnAssetLoadFailed?.Invoke(key, ex);
                Addressables.Release(handle);
                throw ex;
            }
            var asset = handle.Result;
            this.assetCache[key] = (asset, handle, 1);
            progress?.Report(1f);
            this.OnAssetLoaded?.Invoke(key, asset);
            return asset;
        }

        public UniTask<T> LoadAssetAsync<T>(AssetReferenceT<T> reference, IProgress<float> progress = null) where T : Object
        {
            if (reference == null || !reference.RuntimeKeyIsValid()) {
                var ex = new ArgumentException("Invalid asset reference.");
                this.OnAssetLoadFailed?.Invoke(reference?.AssetGUID, ex);
                throw ex;
            }
            return this.LoadAssetAsync<T>(reference.AssetGUID, progress);
        }

        public async UniTask<Dictionary<string, T>> LoadAssetsAsync<T>(IEnumerable<string> keys, IProgress<float> progress = null) where T : Object
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            var keyList = keys.Where(k => !string.IsNullOrEmpty(k)).Distinct().ToList();
            if (keyList.Count == 0) throw new ArgumentException("Keys collection is empty.");
            var   result = new Dictionary<string, T>(keyList.Count);
            float total  = keyList.Count;
            float done   = 0;
            foreach (var key in keyList) {
                var asset                      = await this.LoadAssetAsync<T>(key);
                if (asset != null) result[key] = asset;
                done += 1f;
                progress?.Report(done / total);
            }
            return result;
        }

        public bool TryGet<T>(string key, out T asset) where T : Object
        {
            if (this.assetCache.TryGetValue(key, out var entry) && entry.asset is T t) {
                asset = t;
                return true;
            }
            asset = null;
            return false;
        }

        public bool IsLoaded(string key)
        {
            return this.assetCache.ContainsKey(key);
        }

        public int GetRefCount(string key)
        {
            return this.assetCache.TryGetValue(key, out var entry) ? entry.refCount : 0;
        }

        public void Release(string key)
        {
            if (!this.assetCache.TryGetValue(key, out var entry)) return;
            var newRef = Mathf.Max(0, entry.refCount - 1);
            if (newRef <= 0) {
                if (this.assetCache.TryRemove(key, out var removed)) Addressables.Release(removed.handle);
            }
            else {
                var current = entry;
                this.assetCache.TryUpdate(key, (current.asset, current.handle, newRef), current);
            }
        }

        public void ReleaseAll()
        {
            foreach (var kv in this.assetCache) Addressables.Release(kv.Value.handle);
            this.assetCache.Clear();
            foreach (var dh in this.downloadHandles.Values) Addressables.Release(dh);
            this.downloadHandles.Clear();
        }

        public void Dispose()
        {
            this.ReleaseAll();
        }

        // ------------- Download & Updates -------------
        public async UniTask<long> GetDownloadSizeAsync(IEnumerable<object> keysOrLabels)
        {
            if (keysOrLabels == null) throw new ArgumentNullException(nameof(keysOrLabels));
            var list = keysOrLabels.ToList();
            if (list.Count == 0) throw new ArgumentException("Keys or labels collection is empty.");
            await this.EnsureInitialized();
            var handle = Addressables.GetDownloadSizeAsync(list);
            var size   = await handle.Task.AsUniTask();
            Addressables.Release(handle);
            return size;
        }

        public async UniTask<long> GetDownloadSizeForAllAsync()
        {
            await this.EnsureInitialized();
            var allKeys = this.CollectAllKeys();
            if (allKeys.Count == 0) return 0;
            var handle = Addressables.GetDownloadSizeAsync(allKeys);
            var size   = await handle.Task.AsUniTask();
            Addressables.Release(handle);
            return size;
        }

        public async UniTask<Guid> DownloadDependenciesAsync(IEnumerable<object> keysOrLabels, bool autoReleaseOnComplete = false)
        {
            if (keysOrLabels == null) throw new ArgumentNullException(nameof(keysOrLabels));
            var list = keysOrLabels.ToList();
            if (list.Count == 0) throw new ArgumentException("Keys or labels collection is empty.");
            await this.EnsureInitialized();
            var sizeHandle = Addressables.GetDownloadSizeAsync(list);
            var bytes      = await sizeHandle.Task.AsUniTask();
            Addressables.Release(sizeHandle);
            if (bytes <= 0) return Guid.Empty;
            if (!await this.HasInternetConnectivity()) throw new InvalidOperationException("No network connection available for downloading.");
            var handle = Addressables.DownloadDependenciesAsync(list, Addressables.MergeMode.Union);
            var id     = Guid.NewGuid();
            this.downloadHandles[id] = handle;
            if (autoReleaseOnComplete) {
                await handle.Task.AsUniTask();
                Addressables.Release(handle);
                this.downloadHandles.TryRemove(id, out _);
            }
            return id;
        }

        public async UniTask<Guid> DownloadAllAsync(bool autoReleaseOnComplete = false)
        {
            await this.EnsureInitialized();
            var allKeys = this.CollectAllKeys();
            if (allKeys.Count == 0) return Guid.Empty;
            var sizeHandle = Addressables.GetDownloadSizeAsync(allKeys);
            var totalBytes = await sizeHandle.Task.AsUniTask();
            Addressables.Release(sizeHandle);
            if (totalBytes <= 0) return Guid.Empty;
            if (!await this.HasInternetConnectivity()) throw new InvalidOperationException("No network connection available for downloading.");
            var handle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union);
            var id     = Guid.NewGuid();
            this.downloadHandles[id] = handle;
            if (autoReleaseOnComplete) {
                await handle.Task.AsUniTask();
                Addressables.Release(handle);
                this.downloadHandles.TryRemove(id, out _);
            }
            return id;
        }

        public bool TryGetDownloadStatus(Guid handleId, out DownloadInfo info)
        {
            if (this.downloadHandles.TryGetValue(handleId, out var handle)) {
                var st = handle.GetDownloadStatus();
                info = new() {
                    DownloadedBytes = st.DownloadedBytes,
                    TotalBytes      = st.TotalBytes,
                    Percent         = st.Percent,
                    IsDone          = handle.IsDone,
                    Status          = handle.Status
                };
                return true;
            }
            info = default;
            return false;
        }

        public async UniTask<DownloadInfo> AwaitDownloadAsync(Guid handleId, bool autoRelease = true)
        {
            if (!this.downloadHandles.TryGetValue(handleId, out var handle)) throw new KeyNotFoundException("Download handle not found.");
            await handle.Task.AsUniTask();
            var st = handle.GetDownloadStatus();
            var info = new DownloadInfo {
                DownloadedBytes = st.DownloadedBytes,
                TotalBytes      = st.TotalBytes,
                Percent         = st.Percent,
                IsDone          = handle.IsDone,
                Status          = handle.Status
            };
            if (autoRelease) {
                Addressables.Release(handle);
                this.downloadHandles.TryRemove(handleId, out _);
            }
            return info;
        }

        public void ReleaseDownload(Guid handleId)
        {
            if (this.downloadHandles.TryRemove(handleId, out var handle)) Addressables.Release(handle);
        }

        public async UniTask<bool> CheckCatalogUpdatesAsync()
        {
            await this.EnsureInitialized();
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            var catalogs    = await checkHandle.Task.AsUniTask();
            Addressables.Release(checkHandle);
            return catalogs != null && catalogs.Any();
        }

        public async UniTask<int> UpdateCatalogsAsync()
        {
            await this.EnsureInitialized();
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            var catalogs    = await checkHandle.Task.AsUniTask();
            Addressables.Release(checkHandle);
            if (catalogs == null || catalogs.Count == 0) return 0;
            var updateHandle    = Addressables.UpdateCatalogs(catalogs);
            var updatedLocators = await updateHandle.Task.AsUniTask();
            Addressables.Release(updateHandle);
            this.OnCatalogUpdated?.Invoke();
            return updatedLocators?.Count ?? 0;
        }

        // ------------- Helpers -------------
        private List<object> CollectAllKeys()
        {
            var set = new HashSet<object>();
            foreach (var locator in Addressables.ResourceLocators)
            foreach (var key in locator.Keys)
                set.Add(key);
            return set.ToList();
        }

        private async UniTask<bool> HasInternetConnectivity()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return false;
            try {
                using var req = UnityWebRequest.Head("https://www.google.com");
                req.timeout = 5;
                var op = req.SendWebRequest();
                while (!op.isDone) await UniTask.Yield();
                #if UNITY_2020_2_OR_NEWER
                return req.result == UnityWebRequest.Result.Success;
                #else
                return !req.isNetworkError && !req.isHttpError;
                #endif
            }
            catch {
                return false;
            }
        }
    }
}
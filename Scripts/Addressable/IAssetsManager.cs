namespace GameFoundation.Scripts.Addressable
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using Object = UnityEngine.Object;

    public interface IAssetsManager : IDisposable
    {
        // Load
        T                              LoadAsset<T>(string                    key) where T : Object;
        UniTask<T>                     LoadAssetAsync<T>(string               key,       IProgress<float> progress = null) where T : Object;
        UniTask<T>                     LoadAssetAsync<T>(AssetReferenceT<T>   reference, IProgress<float> progress = null) where T : Object;
        UniTask<Dictionary<string, T>> LoadAssetsAsync<T>(IEnumerable<string> keys,      IProgress<float> progress = null) where T : Object;
        bool                           TryGet<T>(string                       key,       out T            asset) where T : Object;
        bool                           IsLoaded(string                        key);
        int                            GetRefCount(string                     key);
        void                           Release(string                         key);
        void                           ReleaseAll();

        // Download & Updates
        UniTask<long>         GetDownloadSizeAsync(IEnumerable<object> keysOrLabels);
        UniTask<long>         GetDownloadSizeForAllAsync();
        UniTask<Guid>         DownloadDependenciesAsync(IEnumerable<object> keysOrLabels, bool autoReleaseOnComplete = false);
        UniTask<Guid>         DownloadAllAsync(bool                         autoReleaseOnComplete = false);
        bool                  TryGetDownloadStatus(Guid                     handleId, out DownloadInfo info);
        UniTask<DownloadInfo> AwaitDownloadAsync(Guid                       handleId, bool             autoRelease = true);
        void                  ReleaseDownload(Guid                          handleId);
        UniTask<bool>         CheckCatalogUpdatesAsync();
        UniTask<int>          UpdateCatalogsAsync();

        // Events
        event Action<string, Object>    OnAssetLoaded;
        event Action<string, Exception> OnAssetLoadFailed;
        event Action                    OnCatalogUpdated;
    }

    public struct DownloadInfo
    {
        public long                 DownloadedBytes;
        public long                 TotalBytes;
        public float                Percent;
        public bool                 IsDone;
        public AsyncOperationStatus Status;
    }
}
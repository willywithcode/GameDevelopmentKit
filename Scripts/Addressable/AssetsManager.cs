namespace GameFoundation.Scripts.Addressable
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class AssetsManager : IAssetsManager
    {
        private readonly Dictionary<string, object> caches = new();

        public T LoadAsset<T>(string key) where T : Object
        {
            if (!this.caches.ContainsKey(key))
            {
                var newObject = Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
                this.caches.Add(key, newObject);
            }
            return this.caches[key] as T;
        }

        public async UniTask<T> LoadAssetAsync<T>(string key) where T : Object
        {
            if (!this.caches.ContainsKey(key))
            {
                var newObject = await Addressables.LoadAssetAsync<T>(key).Task.AsUniTask();
                this.caches.Add(key, newObject);
            }
            return this.caches[key] as T;
        }
    }
}
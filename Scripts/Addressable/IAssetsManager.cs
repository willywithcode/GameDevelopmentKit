namespace GameFoundation.Scripts.Addressable
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IAssetsManager
    {
        public T          LoadAsset<T>(string      key) where T : Object;
        public UniTask<T> LoadAssetAsync<T>(string key) where T : Object;
    }
}
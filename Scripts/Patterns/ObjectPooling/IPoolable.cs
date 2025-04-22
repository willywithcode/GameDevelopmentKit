namespace GameFoundation.Scripts.Patterns.ObjectPooling
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IPoolable
    {
        public string    key { get; set; }
        public UniTask   OnSpawn();
        public UniTask   OnDespawn();
        public Transform tf { get; }
    }
}
namespace GameFoundation.Scripts.Patterns.ObjectPooling
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public class CommonPoolElement : MonoBehaviour, IPoolable
    {
        public string key { get; set; }

        public virtual UniTask OnInstantiate()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnSpawn()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnDespawn()
        {
            return UniTask.CompletedTask;
        }

        public Transform tf => this.transform;
    }
}
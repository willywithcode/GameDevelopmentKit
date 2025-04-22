namespace GameFoundation.Scripts.Patterns.ObjectPooling
{
    using UnityEngine;

    public interface IObjectPooling
    {
        public void CreatePool<T>(string key, int capacity) where T : Object, IPoolable;
        public T    Spawn<T>(string      key) where T : Object, IPoolable;
        public void Despawn(IPoolable pooler) ;
        public void DespawnAll<T>() where T : Object, IPoolable;
    }
}
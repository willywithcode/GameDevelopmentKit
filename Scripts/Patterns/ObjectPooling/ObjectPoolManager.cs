namespace GameFoundation.Scripts.Patterns.ObjectPooling
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using UnityEngine;
    using VContainer;
    using ZLinq;
    using Object = UnityEngine.Object;

    public class ObjectPoolManager : IObjectPooling
    {
        #region Inject

        private readonly IAssetsManager  assetsManager;
        private readonly IObjectResolver objectResolver;

        public ObjectPoolManager(
            IAssetsManager  assetsManager,
            IObjectResolver objectResolver
        )
        {
            this.assetsManager  = assetsManager;
            this.objectResolver = objectResolver;
        }

        #endregion

        private Transform                                                         pool;
        private Dictionary<string, (Transform parent, List<IPoolable> elements)> pools = new();

        public void CreatePool<T>(string key, int capacity = 10) where T : Object, IPoolable
        {
            if (this.pool is null)
            {
                this.pool = new GameObject("Pool").transform;
                GameObject.DontDestroyOnLoad(this.pool.gameObject);
            }
            if (!this.pools.ContainsKey(key))
            {
                var pool = new GameObject(typeof(T).Name + "_" + key).transform;
                pool.SetParent(this.pool);
                this.pools.Add(key, (pool, new()));
            }
            var pooler = this.assetsManager.LoadAsset<GameObject>(key);
            for (var i = 0; i < capacity; i++)
            {
                var instance = Object.Instantiate(pooler, this.pools[key].parent);
                var objPool = instance.GetComponent<T>();
                this.objectResolver.Inject(objPool);
                objPool.key = key;
                objPool.OnInstantiate();
                instance.gameObject.SetActive(false);
                this.pools[key].elements.Add(objPool);
            }
        }

        public T Spawn<T>(string key) where T : Object, IPoolable
        {
            if (!this.pools.ContainsKey(key)) this.CreatePool<T>(key);
            var inactiveObjs = this.pools[key].elements.AsValueEnumerable().Where(obj => !obj.tf.gameObject.activeSelf);
            if (!inactiveObjs.Any()) this.CreatePool<T>(key, 1);
            inactiveObjs = this.pools[key].elements.AsValueEnumerable().Where(obj => !obj.tf.gameObject.activeSelf);
            var obj = inactiveObjs.First();
            obj.tf.gameObject.SetActive(true);

            return obj as T;
        }
        public T Spawn<T>(string key, Transform parent) where T : Object, IPoolable
        {
            var obj = this.Spawn<T>(key);
            obj.tf.SetParent(parent);
            return obj;
        }
        public T Spawn<T>(string key, Vector3 position, Quaternion rotation) where T : Object, IPoolable
        {
            var obj = this.Spawn<T>(key);
            obj.tf.SetPositionAndRotation(position, rotation);
            return obj;
        }

        public void Despawn(IPoolable pooler)
        {
            if (!this.pools.ContainsKey(pooler.key))
            {
                Debug.LogError("Pooler not found" + pooler.key);
                return;
            }
            pooler.OnDespawn().Forget();
            pooler.tf.gameObject.SetActive(false);
            pooler.tf.SetParent(this.pools[pooler.key].parent);
        }

        public void DespawnAll<T>(string key) where T : Object, IPoolable
        {
            if (!this.pools.ContainsKey(key)) return;
            foreach (var obj in this.pools[key].elements)
            {
                obj.tf.gameObject.SetActive(false);
                obj.tf.SetParent(this.pools[key].parent);
                obj.OnDespawn().Forget();
            }
        }

        public bool IsInitialized(string key)
        {
            if (this.pools.ContainsKey(key))
            {
                return this.pools[key].elements.Count > 0;
            }
            return false;
        }

        public List<T> GetAll<T>(string key)
        {
            if (this.pools.ContainsKey(key))
            {
                var list = new List<T>();
                foreach (var obj in this.pools[key].elements)
                {
                    if (obj.tf.gameObject.activeSelf)
                    {
                        if (obj is T tObj)
                        {
                            list.Add(tObj);
                        }
                    }
                }
                return list;
            }
            Debug.LogError("Pooler not found" + key);
            return new List<T>();
        }
    }
}
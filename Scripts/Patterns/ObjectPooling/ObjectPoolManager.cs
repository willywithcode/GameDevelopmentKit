namespace GameFoundation.Scripts.Patterns.ObjectPooling
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using UnityEngine;
    using ZLinq;
    using Object = UnityEngine.Object;

    public class ObjectPoolManager : IObjectPooling
    {
        #region Inject

        private readonly IAssetsManager assetsManager;

        public ObjectPoolManager(IAssetsManager assetsManager)
        {
            this.assetsManager = assetsManager;
        }

        #endregion

        private Transform                                                         pool;
        private Dictionary<string, (Transform parent, List<GameObject> elements)> pools = new();

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
                instance.GetComponent<T>().key = key;
                var objInstance = instance.gameObject;
                objInstance.SetActive(false);
                this.pools[key].elements.Add(objInstance);
            }
        }

        public T Spawn<T>(string key) where T : Object, IPoolable
        {
            if (!this.pools.ContainsKey(key)) this.CreatePool<T>(key);
            var inactiveObjs = this.pools[key].elements.AsValueEnumerable().Where(obj => !obj.activeSelf);
            if (!inactiveObjs.Any()) this.CreatePool<T>(key);
            inactiveObjs = this.pools[key].elements.AsValueEnumerable().Where(obj => !obj.activeSelf);
            var obj = inactiveObjs.First();
            obj.SetActive(true);
            obj.GetComponent<IPoolable>().OnSpawn().Forget();
            return obj.GetComponent<T>();
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

        public void DespawnAll<T>() where T : Object, IPoolable
        {
            if (!this.pools.ContainsKey(typeof(T).Name)) return;
            foreach (var obj in this.pools[typeof(T).Name].elements)
            {
                obj.SetActive(false);
                obj.transform.SetParent(this.pools[typeof(T).Name].parent);
                obj.GetComponent<IPoolable>().OnDespawn().Forget();
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
                    if (obj.activeSelf)
                    {
                        list.Add(obj.GetComponent<T>());
                    }
                }
                return list;
            }
            Debug.LogError("Pooler not found" + key);
            return new List<T>();
        }
    }
}
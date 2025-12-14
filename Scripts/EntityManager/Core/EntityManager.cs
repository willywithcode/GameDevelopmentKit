namespace GameFoundation.Scripts.EntityManager.Core
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using UnityEngine;
    using VContainer;
    using VContainer.Unity;
    using ZLinq;
    using Object = UnityEngine.Object;

    public class EntityManager : IEntityManager, ITickable
    {
        #region Inject

        private readonly IAssetsManager  assetsManager;
        private readonly IObjectResolver objectResolver;

        public EntityManager(
            IAssetsManager  assetsManager,
            IObjectResolver objectResolver
        )
        {
            this.assetsManager  = assetsManager;
            this.objectResolver = objectResolver;
        }

        #endregion

        private Transform                                                         poolRoot;
        private Dictionary<string, (Transform parent, List<GameObject> elements)> pools    = new();
        private Dictionary<int, IEntity>                                          entities = new();
        private Dictionary<Type, IComponentArray>                                 componentArrays = new();
        private int                                                               nextId;

        #region Pool Management

        private void EnsurePoolRoot()
        {
            if (this.poolRoot != null) return;
            this.poolRoot = new GameObject("EntityPool").transform;
            Object.DontDestroyOnLoad(this.poolRoot.gameObject);
        }

        private void CreatePool<T>(string key, int capacity = 10) where T : Object, IEntity
        {
            this.EnsurePoolRoot();

            if (!this.pools.ContainsKey(key))
            {
                var poolParent = new GameObject(typeof(T).Name + "_" + key).transform;
                poolParent.SetParent(this.poolRoot);
                this.pools.Add(key, (poolParent, new List<GameObject>()));
            }

            var prefab = this.assetsManager.LoadAsset<GameObject>(key);
            for (var i = 0; i < capacity; i++)
            {
                var instance = Object.Instantiate(prefab, this.pools[key].parent);
                var entity   = instance.GetComponent<T>();

                this.objectResolver.Inject(entity);
                entity.Key = key;
                entity.Id  = this.nextId++;
                entity.OnInstantiate().Forget();

                this.entities[entity.Id] = entity;

                instance.SetActive(false);
                this.pools[key].elements.Add(instance);
            }
        }

        #endregion

        #region Spawn

        public T Spawn<T>(string key) where T : Object, IEntity
        {
            if (!this.pools.ContainsKey(key)) this.CreatePool<T>(key);

            var inactiveObjs = this.pools[key].elements.AsValueEnumerable().Where(obj => !obj.activeSelf);
            if (!inactiveObjs.Any()) this.CreatePool<T>(key, 1);

            inactiveObjs = this.pools[key].elements.AsValueEnumerable().Where(obj => !obj.activeSelf);
            var obj    = inactiveObjs.First();
            var entity = obj.GetComponent<T>();

            obj.SetActive(true);
            entity.OnSpawn().Forget();

            return entity;
        }

        public T Spawn<T>(string key, Vector3 position, Quaternion rotation) where T : Object, IEntity
        {
            var entity = this.Spawn<T>(key);
            entity.tf.SetPositionAndRotation(position, rotation);
            return entity;
        }

        #endregion

        #region Despawn

        public void Despawn(IEntity entity)
        {
            if (!this.pools.ContainsKey(entity.Key))
            {
                Debug.LogError($"[EntityManager] Entity pool not found: {entity.Key}");
                return;
            }

            entity.OnDespawn().Forget();
            entity.tf.gameObject.SetActive(false);
            entity.tf.SetParent(this.pools[entity.Key].parent);
        }

        public void DespawnAll<T>() where T : Object, IEntity
        {
            var key = typeof(T).Name;
            if (!this.pools.ContainsKey(key)) return;

            foreach (var obj in this.pools[key].elements)
            {
                if (!obj.activeSelf) continue;

                obj.SetActive(false);
                obj.transform.SetParent(this.pools[key].parent);
                obj.GetComponent<IEntity>().OnDespawn().Forget();
            }
        }

        #endregion

        #region Query

        public bool IsInitialized(string key)
        {
            return this.pools.ContainsKey(key) && this.pools[key].elements.Count > 0;
        }

        public List<T> GetAll<T>(string key) where T : Object, IEntity
        {
            if (!this.pools.ContainsKey(key))
            {
                Debug.LogError($"[EntityManager] Entity pool not found: {key}");
                return new List<T>();
            }

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

        public IEntity GetById(int id)
        {
            return this.entities.TryGetValue(id, out var entity) ? entity : null;
        }

        public List<IEntity> GetEntitiesWithComponent<T>() where T : struct, IComponent
        {
            var result = new List<IEntity>();
            foreach (var kvp in this.entities)
            {
                if (kvp.Value.IsActive && kvp.Value.HasComponent<T>())
                {
                    result.Add(kvp.Value);
                }
            }
            return result;
        }
        public void Tick(float deltaTime)
        {
            foreach (var kvp in this.entities)
            {
                if (kvp.Value.IsActive)
                {
                    kvp.Value.OnTick(deltaTime).Forget();
                }
            }
        }

        #endregion

        #region Component Management (Global)

        public ComponentArray<T> GetComponentArray<T>() where T : struct, IComponent
        {
            var type = typeof(T);
            if (!this.componentArrays.TryGetValue(type, out var array))
            {
                array = new ComponentArray<T>();
                this.componentArrays[type] = array;
            }
            return (ComponentArray<T>)array;
        }

        public void AddComponent<T>(int entityId, T component) where T : struct, IComponent
        {
            this.GetComponentArray<T>().Add(entityId, component);
        }

        public ref T GetComponent<T>(int entityId) where T : struct, IComponent
        {
            return ref this.GetComponentArray<T>().Get(entityId);
        }

        public bool HasComponent<T>(int entityId) where T : struct, IComponent
        {
            return this.GetComponentArray<T>().Has(entityId);
        }

        public void RemoveComponent<T>(int entityId) where T : struct, IComponent
        {
            this.GetComponentArray<T>().Remove(entityId);
        }

        #endregion

        #region Tick

        public void Tick()
        {
            var deltaTime = Time.deltaTime;
            foreach (var kvp in this.entities)
            {
                if (kvp.Value.IsActive)
                {
                    kvp.Value.OnTick(deltaTime).Forget();
                }
            }
        }

        #endregion
    }
}
namespace GameFoundation.Scripts.EntityManager.Core
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
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
        private readonly IGameLogger     logger;

        public EntityManager(
            IAssetsManager  assetsManager,
            IObjectResolver objectResolver,
            IGameLogger     logger = null
        )
        {
            this.assetsManager  = assetsManager;
            this.objectResolver = objectResolver;
            this.logger         = logger ?? new LoggerService();
        }

        #endregion

        private Transform                                                         poolRoot;
        private Dictionary<string, (Transform parent, List<IEntity> elements)> pools    = new();
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
                this.pools.Add(key, (poolParent, new List<IEntity>()));
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
                this.pools[key].elements.Add(entity);
                var childrenEntity = instance.GetComponentsInChildren<IEntity>(true);

                foreach (var childEntity in childrenEntity)
                {
                    this.objectResolver.Inject(childEntity);
                }
            }
        }

        #endregion

        #region Spawn

        public T Spawn<T>(string key, bool isUnique = false) where T : Object, IEntity
        {
            if (!this.pools.ContainsKey(key)) this.CreatePool<T>(key, isUnique ? 1 : 10);

            var inactiveObjs = this.pools[key].elements.AsValueEnumerable().Where(obj => !obj.tf.gameObject.activeSelf);
            if (!inactiveObjs.Any()) this.CreatePool<T>(key, 1);

            inactiveObjs = this.pools[key].elements.AsValueEnumerable().Where(obj => !obj.tf.gameObject.activeSelf);
            var obj    = inactiveObjs.First();

            obj.tf.gameObject.SetActive(true);
            obj.OnSpawn().Forget();

            return (T)obj;
        }

        public T Spawn<T>(string key, Vector3 position, Quaternion rotation, bool isUnique = false) where T : Object, IEntity
        {
            var entity = this.Spawn<T>(key, isUnique);
            entity.tf.SetPositionAndRotation(position, rotation);
            return entity;
        }

        #endregion

        #region Despawn

        public void Despawn(IEntity entity)
        {
            if (!this.pools.ContainsKey(entity.Key))
            {
                this.logger.Error($"[EntityManager] Entity pool not found: {entity.Key}");
                return;
            }

            entity.OnDespawn().Forget();
            entity.tf.gameObject.SetActive(false);
            entity.tf.SetParent(this.pools[entity.Key].parent);
        }

        public void DespawnAll<T>(string key) where T : Object, IEntity
        {
            if (!this.pools.ContainsKey(key)) return;

            foreach (var obj in this.pools[key].elements)
            {
                if (!obj.tf.gameObject.activeSelf) continue;

                obj.tf.gameObject.SetActive(false);
                obj.tf.SetParent(this.pools[key].parent);
                obj.OnDespawn().Forget();
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
                this.logger.Error($"[EntityManager] Entity pool not found: {key}");
                return new List<T>();
            }

            var list = new List<T>();
            foreach (var obj in this.pools[key].elements)
            {
                if (obj.tf.gameObject.activeSelf)
                {
                    list.Add(obj as T);
                }
            }
            return list;
        }

        public IEntity GetById(int id)
        {
            return this.entities.GetValueOrDefault(id);
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

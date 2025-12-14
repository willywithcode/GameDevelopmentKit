namespace GameFoundation.Scripts.EntityManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    public abstract class EntityBase : MonoBehaviour, IEntity
    {
        private Transform cachedTransform;

        public int    Id       { get; set; }
        public string Key      { get; set; }
        public bool   IsActive { get => this.gameObject.activeSelf; }
        public Transform tf
        {
            get
            {
                if (!this.cachedTransform)
                {
                    this.cachedTransform = this.transform;
                }
                return this.cachedTransform;
            }
        }
        private readonly Dictionary<Type, object> components = new Dictionary<Type, object>();

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
        public virtual UniTask OnTick(float deltaTime)
        {
            return UniTask.CompletedTask;
        }

        public void AddComponent<T>(T component) where T : struct, IComponent
        {
            this.components[typeof(T)] = component;
        }

        public ref T GetComponent<T>() where T : struct, IComponent
        {
            return ref Unsafe.Unbox<T>(this.components[typeof(T)]);
        }

        public bool HasComponent<T>() where T : struct, IComponent
        {
            return this.components.ContainsKey(typeof(T));
        }

        public void RemoveComponent<T>() where T : struct, IComponent
        {
            this.components.Remove(typeof(T));
        }

    }
}
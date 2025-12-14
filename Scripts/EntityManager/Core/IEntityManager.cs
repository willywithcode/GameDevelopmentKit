namespace GameFoundation.Scripts.EntityManager.Core
{
    using System.Collections.Generic;
    using UnityEngine;
    public interface IEntityManager
    {
        T       Spawn<T>(string      key) where T : Object, IEntity;
        T       Spawn<T>(string      key, Vector3 position, Quaternion rotation) where T : Object, IEntity;
        void    Despawn(IEntity      entity);
        void    DespawnAll<T>() where T : Object, IEntity;
        bool    IsInitialized(string key);
        List<T> GetAll<T>(string     key) where T : Object, IEntity;

        // ECS-Lite additions
        IEntity       GetById(int id);
        List<IEntity> GetEntitiesWithComponent<T>() where T : struct, IComponent;
        void          Tick(float deltaTime); // Update all entities

    }
}
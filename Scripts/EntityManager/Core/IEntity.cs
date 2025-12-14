namespace GameFoundation.Scripts.EntityManager.Core
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    public interface IEntity
    {
        int       Id       { get; set; }
        string    Key      { get; set; }
        bool      IsActive { get; }
        UniTask   OnInstantiate();
        UniTask   OnSpawn();
        UniTask   OnDespawn();
        UniTask   OnTick(float deltaTime);
        Transform tf { get; }
        void      AddComponent<T>(T component) where T : struct, IComponent;
        ref T     GetComponent<T>() where T : struct, IComponent;
        bool      HasComponent<T>() where T : struct, IComponent;
        void      RemoveComponent<T>() where T : struct, IComponent;

    }
}
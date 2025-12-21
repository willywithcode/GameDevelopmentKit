namespace GameFoundation.Scripts.Blueprints.ScriptableObject.Services
{
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Attributes;
    using UnityEngine;
    using VContainer.Unity;
    public abstract class BaseSOBlueprintService<T> : IInitializable where T : ScriptableObject
    {
        private readonly IAssetsManager assetsManager;
        protected BaseSOBlueprintService(IAssetsManager assetsManager)
        {
            this.assetsManager = assetsManager;

        }
        protected T blueprint;
        public void Initialize()
        {
            var type       = this.GetType();
            var attributes = (SOBlueprintAttributes)type.GetCustomAttributes(typeof(SOBlueprintAttributes), true)[0];
            this.blueprint = this.assetsManager.LoadAsset<T>(attributes.BlueprintName);
        }
        public T GetBlueprint() => this.blueprint;
    }
}
namespace GameFoundation.Scripts.DI
{
    using UnityEngine;
    using VContainer;

    public static class DIExtensions
    {
        private static SceneScope? CurrentSceneContext;

        public static IObjectResolver GetCurrentContainer()
        {
            if (CurrentSceneContext == null) CurrentSceneContext = Object.FindObjectOfType<SceneScope>();
            return CurrentSceneContext.Container.Resolve<IObjectResolver>();
        }

        public static IObjectResolver GetCurrentContainer(this object _)
        {
            return GetCurrentContainer();
        }
    }
}
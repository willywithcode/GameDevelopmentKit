namespace GameFoundation.Scripts.Patterns.MVP.DI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GameFoundation.Scripts.Patterns.MVP.Attribute;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.Screen;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using UnityEngine;
    using VContainer;
    using VContainer.Unity;

    public static class MVPVContainer
    {
        private static readonly List<Type> PresenterTypesToAutoInit = new();

        public static void RegisterMVP(this IContainerBuilder builder)
        {
            builder.Register<ViewFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ScreenManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            RegisterAllPresenters(builder);
            if (PresenterTypesToAutoInit.Count > 0)
            {
                builder.Register<PresenterAutoInitializer>(Lifetime.Singleton)
                    .WithParameter("presenterTypes", PresenterTypesToAutoInit)
                    .AsImplementedInterfaces();
            }
        }

        private static void RegisterAllPresenters(IContainerBuilder builder)
        {
            var basePresenterType = typeof(IPresenter);
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return Type.EmptyTypes;
                    }
                })
                .Where(type =>
                    type != null && !type.IsAbstract && !type.IsInterface && basePresenterType.IsAssignableFrom(type))
                .ToList();

            foreach (var presenterType in allTypes)
            {
                try
                {
                    if (Attribute.GetCustomAttribute(presenterType, typeof(PresenterAttribute)) is PresenterAttribute attribute)
                    {
                        var lifetime = attribute.IsSingleton ? Lifetime.Singleton : Lifetime.Transient;
                        builder.Register(presenterType, lifetime).AsSelf().AsImplementedInterfaces();

                        if (attribute.AutoInit)
                        {
                            PresenterTypesToAutoInit.Add(presenterType);
                        }

                        Debug.Log($"Registered presenter with attribute: {presenterType.Name} (Singleton: {attribute.IsSingleton}, AutoInit: {attribute.AutoInit})");
                    }
                    else
                    {
                        builder.Register(presenterType, Lifetime.Transient).AsSelf().AsImplementedInterfaces();
                        Debug.Log($"Registered presenter: {presenterType.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to register presenter {presenterType.Name}: {ex.Message}");
                }
            }
        }
    }

    // Auto-initializer for presenters marked with AutoInit
    public class PresenterAutoInitializer : IInitializable
    {
        private readonly IObjectResolver resolver;
        private readonly List<Type>      presenterTypes;

        public PresenterAutoInitializer(IObjectResolver resolver, List<Type> presenterTypes)
        {
            this.resolver       = resolver;
            this.presenterTypes = presenterTypes;
        }

        public void Initialize()
        {
            foreach (var presenterType in this.presenterTypes)
            {
                try
                {
                    this.resolver.Resolve(presenterType);
                    Debug.Log($"Auto-initialized presenter: {presenterType.Name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to auto-initialize presenter {presenterType.Name}: {ex.Message}");
                }
            }
        }
    }
}
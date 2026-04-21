namespace GameFoundation.Scripts.Patterns.MVP.DI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.Patterns.MVP.Attribute;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.Screen;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using VContainer;
    using VContainer.Unity;

    public static class MVPVContainer
    {
        private static readonly IGameLogger Logger = new LoggerService();
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

                        Logger.Info($"Registered presenter with attribute: {presenterType.Name} (Singleton: {attribute.IsSingleton}, AutoInit: {attribute.AutoInit})");
                    }
                    else
                    {
                        builder.Register(presenterType, Lifetime.Transient).AsSelf().AsImplementedInterfaces();
                        Logger.Info($"Registered presenter: {presenterType.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to register presenter {presenterType.Name}: {ex.Message}");
                }
            }
        }
    }

    // Auto-initializer for presenters marked with AutoInit
    public class PresenterAutoInitializer : IInitializable
    {
        private readonly IObjectResolver resolver;
        private readonly List<Type>      presenterTypes;
        private readonly IGameLogger     logger;

        public PresenterAutoInitializer(IObjectResolver resolver, List<Type> presenterTypes, IGameLogger logger = null)
        {
            this.resolver       = resolver;
            this.presenterTypes = presenterTypes;
            this.logger         = logger ?? new LoggerService();
        }

        public void Initialize()
        {
            foreach (var presenterType in this.presenterTypes)
            {
                try
                {
                    this.resolver.Resolve(presenterType);
                    this.logger.Info($"Auto-initialized presenter: {presenterType.Name}");
                }
                catch (Exception ex)
                {
                    this.logger.Error($"Failed to auto-initialize presenter {presenterType.Name}: {ex.Message}");
                }
            }
        }
    }
}

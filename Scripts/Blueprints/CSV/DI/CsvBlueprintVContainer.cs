namespace GameFoundation.Scripts.Blueprints.CSV.DI
{
    using GameFoundation.Scripts.Blueprints.CSV.Interfaces;
    using GameFoundation.Scripts.Blueprints.CSV.Services;
    using GameFoundation.Scripts.Utils;
    using VContainer;

    public static class CsvBlueprintVContainer
    {
        public static void RegisterCsvBlueprint(this IContainerBuilder builder)
        {
            var blueprintReaderType = typeof(ICsvBlueprintReader);
            var blueprintTypes      = ReflectionExtensions.GetAllImplementationsOf(blueprintReaderType);

            foreach (var blueprintType in blueprintTypes)
            {
                builder.Register(blueprintType, Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            }

            builder.Register<CsvBlueprintLoader>(Lifetime.Singleton).AsSelf().As<ICsvBlueprintLoader>();
        }
    }
}

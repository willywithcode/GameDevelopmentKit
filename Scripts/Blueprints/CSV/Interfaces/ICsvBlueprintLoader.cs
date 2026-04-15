namespace GameFoundation.Scripts.Blueprints.CSV.Interfaces
{
    using Cysharp.Threading.Tasks;

    public interface ICsvBlueprintLoader
    {
        bool IsLoaded { get; }

        UniTask LoadAllBlueprints();

        UniTask LoadBlueprint<TBlueprint>() where TBlueprint : class, ICsvBlueprintReader;
    }
}

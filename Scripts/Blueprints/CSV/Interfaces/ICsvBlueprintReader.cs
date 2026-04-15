namespace GameFoundation.Scripts.Blueprints.CSV.Interfaces
{
    using Cysharp.Threading.Tasks;

    public interface ICsvBlueprintReader
    {
        UniTask DeserializeFromCsv(string rawCsv);
    }
}

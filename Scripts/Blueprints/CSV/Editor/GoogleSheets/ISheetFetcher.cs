namespace GameFoundation.Scripts.Blueprints.CSV.Editor.GoogleSheets
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISheetFetcher
    {
        Task<SheetFetchResult> FetchAsync(string url, CancellationToken token = default);
    }
}

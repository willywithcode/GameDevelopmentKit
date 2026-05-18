namespace GameFoundation.Scripts.Blueprints.CSV.Editor.GoogleSheets
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class PublicSheetFetcher : ISheetFetcher
    {
        private static readonly HttpClient HttpClient = new();

        public async Task<SheetFetchResult> FetchAsync(string url, CancellationToken token = default)
        {
            try
            {
                var response = await HttpClient.GetAsync(url, token);
                if (!response.IsSuccessStatusCode)
                    return SheetFetchResult.Fail($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");

                var content = await response.Content.ReadAsStringAsync();
                return SheetFetchResult.Ok(content);
            }
            catch (OperationCanceledException)
            {
                return SheetFetchResult.Fail("Cancelled.");
            }
            catch (Exception ex)
            {
                return SheetFetchResult.Fail(ex.Message);
            }
        }
    }
}

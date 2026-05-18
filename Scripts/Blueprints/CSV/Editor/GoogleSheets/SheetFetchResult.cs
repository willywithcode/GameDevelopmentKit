namespace GameFoundation.Scripts.Blueprints.CSV.Editor.GoogleSheets
{
    public class SheetFetchResult
    {
        private SheetFetchResult(bool success, string content, string error)
        {
            this.Success = success;
            this.Content = content;
            this.Error   = error;
        }

        public bool   Success { get; }
        public string Content { get; }
        public string Error   { get; }

        public static SheetFetchResult Ok(string content)  => new(true,  content, null);
        public static SheetFetchResult Fail(string error)  => new(false, null,    error);
    }
}

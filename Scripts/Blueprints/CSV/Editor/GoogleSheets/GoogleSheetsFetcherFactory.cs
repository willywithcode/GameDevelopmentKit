namespace GameFoundation.Scripts.Blueprints.CSV.Editor.GoogleSheets
{
    using System;

    public enum SheetAuthMode
    {
        Public,
        // OAuth,           // future
        // ServiceAccount,  // future
    }

    public static class GoogleSheetsFetcherFactory
    {
        public static ISheetFetcher Create(SheetAuthMode mode) => mode switch
        {
            SheetAuthMode.Public => new PublicSheetFetcher(),
            _ => throw new NotSupportedException($"Auth mode '{mode}' is not supported yet.")
        };
    }
}

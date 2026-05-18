namespace GameFoundation.Scripts.Blueprints.CSV.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class GoogleSheetSourceAttribute : Attribute
    {
        public GoogleSheetSourceAttribute(string spreadsheetId, long sheetGid = 0)
        {
            if (string.IsNullOrWhiteSpace(spreadsheetId))
                throw new ArgumentException("Spreadsheet ID must not be empty.", nameof(spreadsheetId));

            this.SpreadsheetId = spreadsheetId;
            this.SheetGid      = sheetGid;
        }

        public string SpreadsheetId { get; }
        public long   SheetGid      { get; }

        public string BuildExportUrl() =>
            $"https://docs.google.com/spreadsheets/d/{this.SpreadsheetId}/export?format=csv&gid={this.SheetGid}";
    }
}

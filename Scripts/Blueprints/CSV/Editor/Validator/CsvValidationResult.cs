namespace GameFoundation.Scripts.Blueprints.CSV.Editor.Validator
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Blueprints.CSV.Attributes;

    public enum ValidationStatus { Ok, Warning, Error }

    public class CsvIssue
    {
        public CsvIssue(ValidationStatus severity, string message, string location = null)
        {
            this.Severity = severity;
            this.Message  = message;
            this.Location = location;
        }

        public ValidationStatus Severity { get; }
        public string           Message  { get; }
        public string           Location { get; } // null | "Column: X" | "Row 3, Column: X"
    }

    public class CsvBlueprintValidationResult
    {
        public CsvBlueprintValidationResult(Type blueprintType, string dataPath, CsvBlueprintSource source)
        {
            this.BlueprintType = blueprintType;
            this.DataPath      = dataPath;
            this.Source        = source;
            this.Issues        = new List<CsvIssue>();
        }

        public Type                BlueprintType { get; }
        public string              DataPath      { get; }
        public CsvBlueprintSource  Source        { get; }
        public List<CsvIssue>      Issues        { get; }

        public ValidationStatus Status
        {
            get
            {
                var worst = ValidationStatus.Ok;
                foreach (var issue in this.Issues)
                    if (issue.Severity > worst) worst = issue.Severity;
                return worst;
            }
        }

        public void AddIssue(ValidationStatus severity, string message, string location = null)
            => this.Issues.Add(new CsvIssue(severity, message, location));
    }

    public class CsvOrphanResult
    {
        public CsvOrphanResult(string assetPath) => this.AssetPath = assetPath;
        public string AssetPath { get; }
    }

    public class CsvValidationReport
    {
        public List<CsvBlueprintValidationResult> BlueprintResults { get; } = new();
        public List<CsvOrphanResult>              Orphans          { get; } = new();
    }
}

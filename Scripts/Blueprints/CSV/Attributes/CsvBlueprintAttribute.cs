namespace GameFoundation.Scripts.Blueprints.CSV.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CsvBlueprintAttribute : Attribute
    {
        public CsvBlueprintAttribute(string dataPath, CsvBlueprintSource source = CsvBlueprintSource.Resources, bool encrypted = false)
        {
            if (string.IsNullOrWhiteSpace(dataPath))
            {
                throw new ArgumentException("CSV blueprint data path must not be empty.", nameof(dataPath));
            }

            this.DataPath  = dataPath;
            this.Source    = source;
            this.Encrypted = encrypted;
        }

        public string DataPath { get; }

        public CsvBlueprintSource Source { get; }

        public bool Encrypted { get; }
    }

    public enum CsvBlueprintSource
    {
        Resources   = 0,
        Addressable = 1
    }
}

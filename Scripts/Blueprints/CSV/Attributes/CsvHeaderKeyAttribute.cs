namespace GameFoundation.Scripts.Blueprints.CSV.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class CsvHeaderKeyAttribute : Attribute
    {
        public CsvHeaderKeyAttribute(string headerKey)
        {
            if (string.IsNullOrWhiteSpace(headerKey))
            {
                throw new ArgumentException("CSV header key must not be empty.", nameof(headerKey));
            }

            this.HeaderKey = headerKey;
        }

        public string HeaderKey { get; }
    }
}

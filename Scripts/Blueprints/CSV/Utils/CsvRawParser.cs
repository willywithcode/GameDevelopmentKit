namespace GameFoundation.Scripts.Blueprints.CSV.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class CsvRawParser
    {
        public static CsvTable Parse(string rawCsv)
        {
            if (string.IsNullOrWhiteSpace(rawCsv))
            {
                throw new ArgumentException("CSV content is null or empty.", nameof(rawCsv));
            }

            var rows = ParseRows(rawCsv);
            if (rows.Count == 0)
            {
                throw new InvalidOperationException("CSV does not contain any row.");
            }

            var header = rows[0];
            if (header.Count == 0)
            {
                throw new InvalidOperationException("CSV header row is empty.");
            }

            if (!string.IsNullOrEmpty(header[0]))
            {
                header[0] = header[0].TrimStart('\ufeff');
            }

            var bodyRows = rows
                .Skip(1)
                .Where(row => row.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                .Select(row => (IReadOnlyList<string>)row)
                .ToList();

            return new CsvTable(header, bodyRows);
        }

        private static List<List<string>> ParseRows(string rawCsv)
        {
            var rows         = new List<List<string>>();
            var currentRow   = new List<string>();
            var currentField = new StringBuilder();
            var inQuotes     = false;

            for (var i = 0; i < rawCsv.Length; i++)
            {
                var currentChar = rawCsv[i];

                if (currentChar == '"')
                {
                    if (inQuotes && i + 1 < rawCsv.Length && rawCsv[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && currentChar == ',')
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                    continue;
                }

                if (!inQuotes && (currentChar == '\r' || currentChar == '\n'))
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                    rows.Add(currentRow);
                    currentRow = new List<string>();

                    if (currentChar == '\r' && i + 1 < rawCsv.Length && rawCsv[i + 1] == '\n')
                    {
                        i++;
                    }

                    continue;
                }

                currentField.Append(currentChar);
            }

            currentRow.Add(currentField.ToString());

            if (currentRow.Count > 1 || currentRow[0].Length > 0 || rawCsv.EndsWith(",", StringComparison.Ordinal))
            {
                rows.Add(currentRow);
            }

            return rows;
        }
    }

    public class CsvTable
    {
        public CsvTable(IReadOnlyList<string> header, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            this.Header = header ?? throw new ArgumentNullException(nameof(header));
            this.Rows   = rows ?? throw new ArgumentNullException(nameof(rows));
        }

        public IReadOnlyList<string> Header { get; }

        public IReadOnlyList<IReadOnlyList<string>> Rows { get; }
    }
}

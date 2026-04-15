namespace GameFoundation.Scripts.Blueprints.CSV.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Blueprints.CSV.Interfaces;
    using GameFoundation.Scripts.Blueprints.CSV.Utils;

    public abstract class CsvBlueprintReaderByCol : ICsvBlueprintReader
    {
        public virtual UniTask DeserializeFromCsv(string rawCsv)
        {
            var table = CsvRawParser.Parse(rawCsv);
            var memberInfos = CsvReflectionCache
                .GetAllFieldAndProperties(this.GetType())
                .GroupBy(memberInfo => memberInfo.MemberName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var row in table.Rows)
            {
                if (row.Count < 2)
                {
                    continue;
                }

                var key = row[0];
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (!memberInfos.TryGetValue(key, out var memberInfo))
                {
                    continue;
                }

                var rawValue       = row[1];
                var convertedValue = CsvTypeConverter.ConvertFromString(rawValue, memberInfo.MemberType);
                memberInfo.SetValue(this, convertedValue);
            }

            return UniTask.CompletedTask;
        }
    }
}

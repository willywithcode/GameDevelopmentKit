namespace GameFoundation.Scripts.Blueprints.CSV.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Blueprints.CSV.Attributes;
    using GameFoundation.Scripts.Blueprints.CSV.Interfaces;
    using GameFoundation.Scripts.Blueprints.CSV.Utils;
    using ZLinq;

    public abstract class CsvBlueprintReaderByRow<TKey, TRecord> : ICsvBlueprintReader
    {
        private readonly Dictionary<TKey, TRecord> data = new();

        public int   Count                          => this.data.Count;
        public TRecord this[TKey key]               => this.data[key];
        public bool ContainsKey(TKey key)           => this.data.ContainsKey(key);
        public bool TryGetValue(TKey key, out TRecord value) => this.data.TryGetValue(key, out value);

        public virtual UniTask DeserializeFromCsv(string rawCsv)
        {
            this.data.Clear();

            var table = CsvRawParser.Parse(rawCsv);
            var headerLookup = table.Header
                .AsValueEnumerable()
                .Select((header, index) => (header, index))
                .ToDictionary(d => d.header, d => d.index, StringComparer.OrdinalIgnoreCase);

            var recordType = typeof(TRecord);
            var keyField   = this.GetKeyField(recordType);

            if (!headerLookup.TryGetValue(keyField, out var keyFieldIndex))
            {
                throw new InvalidOperationException($"CSV does not contain key column '{keyField}' for blueprint '{this.GetType().Name}'.");
            }

            var memberInfos = CsvReflectionCache.GetAllFieldAndProperties(recordType);
            foreach (var row in table.Rows)
            {
                var rawKey = GetCellValue(row, keyFieldIndex);
                if (string.IsNullOrWhiteSpace(rawKey))
                {
                    continue;
                }

                var record = (TRecord)Activator.CreateInstance(recordType);
                foreach (var memberInfo in memberInfos)
                {
                    if (!headerLookup.TryGetValue(memberInfo.MemberName, out var columnIndex))
                    {
                        continue;
                    }

                    var rawValue       = GetCellValue(row, columnIndex);
                    var convertedValue = CsvTypeConverter.ConvertFromString(rawValue, memberInfo.MemberType);
                    memberInfo.SetValue(record, convertedValue);
                }

                var convertedKey = (TKey)CsvTypeConverter.ConvertFromString(rawKey, typeof(TKey));
                if (this.data.ContainsKey(convertedKey))
                {
                    throw new InvalidOperationException($"Duplicate key '{convertedKey}' in blueprint '{this.GetType().Name}'.");
                }

                this.data.Add(convertedKey, record);
            }

            return UniTask.CompletedTask;
        }

        public TRecord GetDataById(TKey id)
        {
            if (this.data.TryGetValue(id, out var record))
            {
                return record;
            }

            throw new KeyNotFoundException($"Blueprint '{this.GetType().Name}' does not contain key '{id}'.");
        }

        private string GetKeyField(Type recordType)
        {
            var recordAttribute = recordType.GetCustomAttribute<CsvHeaderKeyAttribute>();
            if (recordAttribute != null)
            {
                return recordAttribute.HeaderKey;
            }

            var blueprintAttribute = this.GetType().GetCustomAttribute<CsvHeaderKeyAttribute>();
            if (blueprintAttribute != null)
            {
                return blueprintAttribute.HeaderKey;
            }

            var firstMember = CsvReflectionCache.GetAllFieldAndProperties(recordType).AsValueEnumerable().FirstOrDefault();
            if (firstMember == null)
            {
                throw new InvalidOperationException($"Blueprint record type '{recordType.FullName}' does not contain any public field or property.");
            }

            return firstMember.MemberName;
        }

        private static string GetCellValue(IReadOnlyList<string> row, int index)
        {
            if (index < 0 || index >= row.Count)
            {
                return string.Empty;
            }

            return row[index];
        }
    }
}

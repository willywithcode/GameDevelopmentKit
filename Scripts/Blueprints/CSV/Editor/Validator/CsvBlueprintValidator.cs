namespace GameFoundation.Scripts.Blueprints.CSV.Editor.Validator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using GameFoundation.Scripts.Blueprints.CSV.Attributes;
    using GameFoundation.Scripts.Blueprints.CSV.Interfaces;
    using GameFoundation.Scripts.Blueprints.CSV.Readers;
    using GameFoundation.Scripts.Blueprints.CSV.Utils;
    using GameFoundation.Scripts.Utils;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEngine;

    public static class CsvBlueprintValidator
    {
        private const string AddressablesCsvFolder = "Assets/Blueprints/CSV";
        private const string ResourcesCsvFolder    = "Assets/Resources/Blueprints/CSV";

        public static CsvValidationReport ValidateAll(bool validateDataTypes = false)
        {
            var report       = new CsvValidationReport();
            var knownPaths   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var blueprints   = ReflectionExtensions.GetAllImplementationsOf(typeof(ICsvBlueprintReader));

            foreach (var type in blueprints)
            {
                var result = ValidateBlueprint(type, validateDataTypes, out var assetPath);
                report.BlueprintResults.Add(result);
                if (assetPath != null) knownPaths.Add(assetPath);
            }

            DetectOrphans(knownPaths, report);
            return report;
        }

        private static CsvBlueprintValidationResult ValidateBlueprint(Type type, bool validateDataTypes, out string assetPath)
        {
            assetPath = null;
            var attr = type.GetCustomAttribute<CsvBlueprintAttribute>();
            if (attr == null)
            {
                var r = new CsvBlueprintValidationResult(type, "?", CsvBlueprintSource.Resources);
                r.AddIssue(ValidationStatus.Error, "Missing [CsvBlueprintAttribute]");
                return r;
            }

            var result = new CsvBlueprintValidationResult(type, attr.DataPath, attr.Source);

            var textAsset = LoadCsvAsset(attr, out assetPath);
            if (textAsset == null)
            {
                result.AddIssue(ValidationStatus.Error, $"CSV file not found at '{attr.DataPath}'");
                return result;
            }

            CsvTable table;
            try { table = CsvRawParser.Parse(textAsset.text); }
            catch (Exception ex)
            {
                result.AddIssue(ValidationStatus.Error, $"Failed to parse CSV: {ex.Message}");
                return result;
            }

            if (TryGetRowReaderArgs(type, out _, out var recordType))
                ValidateByRow(result, table, type, recordType, validateDataTypes);
            else if (typeof(CsvBlueprintReaderByCol).IsAssignableFrom(type))
                ValidateByCol(result, table, type);

            return result;
        }

        private static void ValidateByRow(CsvBlueprintValidationResult result, CsvTable table, Type blueprintType, Type recordType, bool validateDataTypes)
        {
            var headerLookup = BuildHeaderLookup(table);

            var keyField = GetKeyField(blueprintType, recordType);
            if (keyField == null)
            {
                result.AddIssue(ValidationStatus.Error, "Cannot determine key field for record type");
                return;
            }

            if (!headerLookup.TryGetValue(keyField, out var keyColumnIndex))
            {
                result.AddIssue(ValidationStatus.Error, $"Key column '{keyField}' not found in CSV header");
                return;
            }

            var members = CsvReflectionCache.GetAllFieldAndProperties(recordType);
            foreach (var member in members)
            {
                if (!headerLookup.ContainsKey(member.MemberName))
                    result.AddIssue(ValidationStatus.Warning, $"Column '{member.MemberName}' not in CSV — field will always be default value");
            }

            var seenKeys = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                var rawKey = GetCellValue(table.Rows[rowIndex], keyColumnIndex);
                if (string.IsNullOrWhiteSpace(rawKey)) continue;

                if (seenKeys.TryGetValue(rawKey, out var firstRow))
                    result.AddIssue(ValidationStatus.Error, $"Duplicate key '{rawKey}' (first seen at row {firstRow + 2})", $"Row {rowIndex + 2}, Column: {keyField}");
                else
                    seenKeys[rawKey] = rowIndex;
            }

            if (!validateDataTypes) return;

            foreach (var member in members)
            {
                if (!headerLookup.TryGetValue(member.MemberName, out var columnIndex)) continue;
                for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                {
                    var rawValue = GetCellValue(table.Rows[rowIndex], columnIndex);
                    if (string.IsNullOrWhiteSpace(rawValue)) continue;
                    try { CsvTypeConverter.ConvertFromString(rawValue, member.MemberType); }
                    catch (Exception ex)
                    {
                        result.AddIssue(ValidationStatus.Error,
                            $"Cannot parse '{rawValue}' as {member.MemberType.Name}: {ex.Message}",
                            $"Row {rowIndex + 2}, Column: {member.MemberName}");
                    }
                }
            }
        }

        private static void ValidateByCol(CsvBlueprintValidationResult result, CsvTable table, Type blueprintType)
        {
            var members      = CsvReflectionCache.GetAllFieldAndProperties(blueprintType);
            var memberNames  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in members) memberNames.Add(m.MemberName);

            foreach (var row in table.Rows)
            {
                if (row.Count < 1) continue;
                var key = row[0];
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (!memberNames.Contains(key))
                    result.AddIssue(ValidationStatus.Warning, $"CSV key '{key}' has no matching field in {blueprintType.Name}");
            }
        }

        private static void DetectOrphans(HashSet<string> knownPaths, CsvValidationReport report)
        {
            var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { AddressablesCsvFolder, ResourcesCsvFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) continue;
                if (!knownPaths.Contains(path))
                    report.Orphans.Add(new CsvOrphanResult(path));
            }
        }

        private static TextAsset LoadCsvAsset(CsvBlueprintAttribute attr, out string assetPath)
        {
            assetPath = null;

            if (attr.Source == CsvBlueprintSource.Addressable)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null) return null;

                foreach (var group in settings.groups)
                {
                    if (group == null) continue;
                    foreach (var entry in group.entries)
                    {
                        if (!string.Equals(entry.address, attr.DataPath, StringComparison.OrdinalIgnoreCase)) continue;
                        assetPath = entry.AssetPath;
                        return AssetDatabase.LoadAssetAtPath<TextAsset>(entry.AssetPath);
                    }
                }
                return null;
            }

            var normalized = NormalizeResourcePath(attr.DataPath);
            assetPath = $"Assets/Resources/{normalized}.csv";
            return AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        }

        private static bool TryGetRowReaderArgs(Type type, out Type keyType, out Type recordType)
        {
            var t = type.BaseType;
            while (t != null)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(CsvBlueprintReaderByRow<,>))
                {
                    keyType    = t.GetGenericArguments()[0];
                    recordType = t.GetGenericArguments()[1];
                    return true;
                }
                t = t.BaseType;
            }
            keyType    = null;
            recordType = null;
            return false;
        }

        private static string GetKeyField(Type blueprintType, Type recordType)
        {
            var recordAttr = recordType.GetCustomAttribute<CsvHeaderKeyAttribute>();
            if (recordAttr != null) return recordAttr.HeaderKey;

            var blueprintAttr = blueprintType.GetCustomAttribute<CsvHeaderKeyAttribute>();
            if (blueprintAttr != null) return blueprintAttr.HeaderKey;

            var members = CsvReflectionCache.GetAllFieldAndProperties(recordType);
            return members.Count > 0 ? members[0].MemberName : null;
        }

        private static Dictionary<string, int> BuildHeaderLookup(CsvTable table)
        {
            var lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < table.Header.Count; i++)
                lookup[table.Header[i]] = i;
            return lookup;
        }

        private static string GetCellValue(IReadOnlyList<string> row, int index)
            => index >= 0 && index < row.Count ? row[index] : string.Empty;

        private static string NormalizeResourcePath(string dataPath)
        {
            var p = dataPath.Trim().Replace('\\', '/').TrimStart('/');
            if (p.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) p = p[..^4];
            if (!p.StartsWith("Blueprints/CSV", StringComparison.OrdinalIgnoreCase)) p = $"Blueprints/CSV/{p}";
            return p;
        }
    }
}

namespace GameFoundation.Scripts.Blueprints.CSV.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEngine;
    using ZLinq;

    public static class CsvTypeConverter
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        public static object ConvertFromString(string rawValue, Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            if (targetType == typeof(string)) return rawValue;

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                {
                    return null;
                }

                return Activator.CreateInstance(targetType);
            }

            var nullableType = Nullable.GetUnderlyingType(targetType);
            if (nullableType != null)
            {
                return ConvertFromString(rawValue, nullableType);
            }

            if (targetType == typeof(bool))
            {
                if (rawValue == "1") return true;
                if (rawValue == "0") return false;

                return bool.Parse(rawValue);
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, rawValue, true);
            }

            if (targetType == typeof(Vector2))
            {
                return ParseVector2(rawValue);
            }

            if (targetType == typeof(Vector3))
            {
                return ParseVector3(rawValue);
            }

            if (TryConvertCollection(rawValue, targetType, out var collectionValue))
            {
                return collectionValue;
            }

            return Convert.ChangeType(rawValue, targetType, InvariantCulture);
        }

        private static bool TryConvertCollection(string rawValue, Type targetType, out object result)
        {
            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType();
                var values      = SplitCollectionValues(rawValue, ',');
                var array       = Array.CreateInstance(elementType, values.Count);

                for (var index = 0; index < values.Count; index++)
                {
                    array.SetValue(ConvertFromString(values[index], elementType), index);
                }

                result = array;
                return true;
            }

            if (!targetType.IsGenericType)
            {
                result = null;
                return false;
            }

            var genericTypeDefinition = targetType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Dictionary<,>) || genericTypeDefinition == typeof(IDictionary<,>))
            {
                result = ConvertDictionary(rawValue, targetType.GetGenericArguments()[0], targetType.GetGenericArguments()[1]);
                return true;
            }

            if (genericTypeDefinition == typeof(List<>) ||
                genericTypeDefinition == typeof(IList<>) ||
                genericTypeDefinition == typeof(IReadOnlyList<>) ||
                genericTypeDefinition == typeof(ICollection<>) ||
                genericTypeDefinition == typeof(IEnumerable<>))
            {
                result = ConvertList(rawValue, targetType.GetGenericArguments()[0]);
                return true;
            }

            result = null;
            return false;
        }

        private static object ConvertDictionary(string rawValue, Type keyType, Type valueType)
        {
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var dictionary     = (IDictionary)Activator.CreateInstance(dictionaryType);
            var pairs          = SplitCollectionValues(rawValue, ',');

            foreach (var pair in pairs)
            {
                var pairData = pair.Split(new[] {':'}, 2, StringSplitOptions.None);
                if (pairData.Length != 2)
                {
                    continue;
                }

                var key   = ConvertFromString(pairData[0].Trim(), keyType);
                var value = ConvertFromString(pairData[1].Trim(), valueType);
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        private static object ConvertList(string rawValue, Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list     = (IList)Activator.CreateInstance(listType);
            var values   = SplitCollectionValues(rawValue, ',');

            foreach (var value in values)
            {
                list.Add(ConvertFromString(value, elementType));
            }

            return list;
        }

        private static List<string> SplitCollectionValues(string rawValue, char delimiter)
        {
            if (rawValue.IndexOf(delimiter) < 0)
            {
                // '|' is an alternative delimiter for collections, used when ',' conflicts with CSV field separator
                if (delimiter != '|' && rawValue.IndexOf('|') >= 0)
                {
                    delimiter = '|';
                }
                else
                {
                    return new List<string> { rawValue.Trim() };
                }
            }

            return rawValue
                .Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
                .AsValueEnumerable()
                .Select(value => value.Trim())
                .ToList();
        }

        private static Vector2 ParseVector2(string rawValue)
        {
            var valueData = rawValue.Split('|');
            if (valueData.Length != 2)
            {
                throw new FormatException($"Can not parse '{rawValue}' to Vector2, expected format: x|y.");
            }

            return new Vector2(
                float.Parse(valueData[0], InvariantCulture),
                float.Parse(valueData[1], InvariantCulture));
        }

        private static Vector3 ParseVector3(string rawValue)
        {
            var valueData = rawValue.Split('|');
            if (valueData.Length != 3)
            {
                throw new FormatException($"Can not parse '{rawValue}' to Vector3, expected format: x|y|z.");
            }

            return new Vector3(
                float.Parse(valueData[0], InvariantCulture),
                float.Parse(valueData[1], InvariantCulture),
                float.Parse(valueData[2], InvariantCulture));
        }
    }
}

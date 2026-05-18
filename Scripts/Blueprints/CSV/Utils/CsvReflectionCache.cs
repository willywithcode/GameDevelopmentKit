namespace GameFoundation.Scripts.Blueprints.CSV.Utils
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using ZLinq;

    public static class CsvReflectionCache
    {
        private static readonly ConcurrentDictionary<Type, List<CsvMemberInfo>> MemberInfosCache = new();

        public static List<CsvMemberInfo> GetAllFieldAndProperties(Type typeInfo)
        {
            if (typeInfo == null) throw new ArgumentNullException(nameof(typeInfo));

            return MemberInfosCache.GetOrAdd(typeInfo, type =>
            {
                var results = new List<CsvMemberInfo>();

                results.AddRange(type
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .AsValueEnumerable()
                    .Where(fieldInfo => !fieldInfo.IsInitOnly)
                    .Select(fieldInfo => new CsvMemberInfo(
                        fieldInfo.Name,
                        fieldInfo.FieldType,
                        fieldInfo.GetValue,
                        fieldInfo.SetValue))
                    .ToList());

                results.AddRange(type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .AsValueEnumerable()
                    .Where(propertyInfo => propertyInfo.CanRead && propertyInfo.CanWrite && propertyInfo.GetIndexParameters().Length == 0)
                    .Select(propertyInfo => new CsvMemberInfo(
                        propertyInfo.Name,
                        propertyInfo.PropertyType,
                        propertyInfo.GetValue,
                        propertyInfo.SetValue))
                    .ToList());

                return results;
            });
        }
    }

    public class CsvMemberInfo
    {
        public CsvMemberInfo(string memberName, Type memberType, Func<object, object> getValue, Action<object, object> setValue)
        {
            this.MemberName = memberName;
            this.MemberType = memberType;
            this.GetValue   = getValue;
            this.SetValue   = setValue;
        }

        public string MemberName { get; }

        public Type MemberType { get; }

        public Func<object, object> GetValue { get; }

        public Action<object, object> SetValue { get; }
    }
}

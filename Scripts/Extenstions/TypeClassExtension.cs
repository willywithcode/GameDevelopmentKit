namespace GameFoundation.Scripts.Extenstions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ZLinq;

    public static class TypeClassExtension
    {
        public static Type[] GetTypesInheritingFrom(this Type baseType)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .AsValueEnumerable()
                .Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract)
                .ToArray();
        }

        public static Type[] GetTypesImplementing(this Type interfaceType)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .AsValueEnumerable()
                .Where(t => t.GetInterfaces().AsValueEnumerable().Contains(interfaceType) && !t.IsAbstract)
                .ToArray();
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            var currentType = type.BaseType;
            while (currentType != null)
            {
                yield return currentType;
                currentType = currentType.BaseType;
            }
        }
    }
}
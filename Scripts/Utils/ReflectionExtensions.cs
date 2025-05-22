namespace GameFoundation.Scripts.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class ReflectionExtensions
    {
        /// <summary>
        ///     Gets all non-abstract classes that implement or inherit from the specified base type
        /// </summary>
        public static List<Type> GetAllImplementationsOf(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return Type.EmptyTypes;
                    }
                })
                .Where(type =>
                    type != null && !type.IsAbstract && !type.IsInterface && (baseType.IsAssignableFrom(type) || (baseType.IsGenericTypeDefinition && IsSubclassOfGenericType(type, baseType))))
                .ToList();
        }

        /// <summary>
        ///     Gets all non-abstract classes that implement the specified generic interface
        /// </summary>
        public static List<Type> GetAllImplementationsOfGenericInterface(Type genericInterfaceType)
        {
            if (!genericInterfaceType.IsGenericTypeDefinition || !genericInterfaceType.IsInterface) throw new ArgumentException("Type must be a generic interface definition", nameof(genericInterfaceType));

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return Type.EmptyTypes;
                    }
                })
                .Where(type =>
                    type != null && !type.IsAbstract && !type.IsInterface && GetImplementedGenericInterfaces(type, genericInterfaceType).Any())
                .ToList();
        }

        /// <summary>
        ///     Gets all implemented interfaces that are the specified generic interface
        /// </summary>
        public static IEnumerable<Type> GetImplementedGenericInterfaces(Type type, Type genericInterfaceType)
        {
            return type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType);
        }

        /// <summary>
        ///     Checks if a type is a subclass of a generic type
        /// </summary>
        public static bool IsSubclassOfGenericType(Type type, Type genericType)
        {
            if (!genericType.IsGenericTypeDefinition)
                return false;

            // Check interfaces
            var interfaces = type.GetInterfaces();
            if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType))
                return true;

            // Check base classes
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericType)
                    return true;
                baseType = baseType.BaseType;
            }

            return false;
        }

        /// <summary>
        ///     Gets the generic arguments of a type that implements a generic interface
        /// </summary>
        public static Type[] GetGenericArguments(Type type, Type genericInterfaceType)
        {
            var implementedInterface = GetImplementedGenericInterfaces(type, genericInterfaceType).FirstOrDefault();
            return implementedInterface?.GetGenericArguments() ?? Type.EmptyTypes;
        }
    }
}
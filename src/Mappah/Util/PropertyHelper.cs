namespace Mappah.Util
{
    using Mappah.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class PropertyHelper
    {
        public static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Cannot create instance of type '{type.FullName}'.");
        }

        public static PropertyInfo[] GetPublicReadableWritableProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();
        }

        public static Dictionary<string, PropertyInfo> BuildPropertyMap(Type type)
        {
            return GetPublicReadableWritableProperties(type)
                .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);
        }

        public static IEnumerable<(PropertyInfo sourceProp, PropertyInfo targetProp)> FindMatchingProperties(
            Dictionary<string, PropertyInfo> sourceProps,
            PropertyInfo[] targetProps,
            HashSet<string> ignoredProperties,
            HashSet<string> overriddenProperties)
        {
            foreach (var targetProp in targetProps)
            {
                if (ignoredProperties.Contains(targetProp.Name) || overriddenProperties.Contains(targetProp.Name))
                    continue;

                if (sourceProps.TryGetValue(targetProp.Name, out var sourceProp))
                {
                    if (IsCompatible(sourceProp.PropertyType, targetProp.PropertyType) ||
                        ShouldMapAsComplexType(sourceProp.PropertyType, targetProp.PropertyType))
                    {
                        yield return (sourceProp, targetProp);
                    }
                }
            }
        }

        public static bool IsCompatible(Type sourceType, Type targetType)
        {
            if (targetType.IsAssignableFrom(sourceType))
                return true;

            var targetUnderlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var sourceUnderlying = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

            if (sourceUnderlying == targetUnderlying)
                return true;

            try
            {
                _ = Convert.ChangeType(Activator.CreateInstance(sourceUnderlying), targetUnderlying);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static object? ConvertValue(object? sourceValue, Type targetType)
        {
            if (sourceValue == null)
                return null;

            var targetUnderlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (targetUnderlying.IsAssignableFrom(sourceValue.GetType()))
                return sourceValue;

            return Convert.ChangeType(sourceValue, targetUnderlying);
        }

        public static bool ShouldMapAsComplexType(Type sourceType, Type targetType)
        {
            if (sourceType == typeof(string) || targetType == typeof(string))
                return false;

            if (sourceType.IsPrimitive || targetType.IsPrimitive)
                return false;

            if (sourceType.IsEnum || targetType.IsEnum)
                return false;

            return MappingConfigurationStore.ReadMappingConfiguration(sourceType, targetType) != null;
        }
    }
}

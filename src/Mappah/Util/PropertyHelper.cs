namespace Mappah.Util
{
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
                .ToDictionary(p => p.Name, p => p);
        }

        public static IEnumerable<(PropertyInfo sourceProp, PropertyInfo targetProp)> FindMatchingProperties(
            Dictionary<string, PropertyInfo> sourceProps,
            PropertyInfo[] targetProps,
            HashSet<string>? excludedProperties = null,
            HashSet<string>? overriddenProperties = null)
        {
            foreach (var targetProp in targetProps)
            {
                if (excludedProperties != null && excludedProperties.Contains(targetProp.Name))
                {
                    continue;
                }

                if (overriddenProperties != null && overriddenProperties.Contains(targetProp.Name))
                {
                    continue;
                }

                if (sourceProps.TryGetValue(targetProp.Name, out var sourceProp))
                {
                    if (IsCompatible(sourceProp.PropertyType, targetProp.PropertyType))
                        yield return (sourceProp, targetProp);
                }
            }
        }

        public static bool IsCompatible(Type sourceType, Type targetType)
        {
            if (targetType.IsAssignableFrom(sourceType))
            {
                return true;
            }

            var targetUnderlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var sourceUnderlying = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

            try
            {
                Convert.ChangeType(Activator.CreateInstance(sourceUnderlying), targetUnderlying);
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
            {
                return null;
            }

            var targetUnderlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (targetUnderlying.IsAssignableFrom(sourceValue.GetType()))
            {
                return sourceValue;
            }

            return Convert.ChangeType(sourceValue, targetUnderlying);
        }
    }
}

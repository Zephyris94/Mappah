using Mappah.Configuration;
using Mappah.Util;
using System.Reflection;

namespace Mappah.Resolution
{
    public sealed class DefaultMapResolver : IMapResolver
    {
        public TDest Map<TDest, TSource>(TSource source)
        {
            ArgumentNullException.ThrowIfNull(source);
            return MapInternal<TDest>(source);
        }

        public IEnumerable<TDest> Map<TDest, TSource>(IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var item in source)
            {
                yield return Map<TDest, TSource>(item);
            }
        }

        public TDest Map<TDest>(object source)
        {
            ArgumentNullException.ThrowIfNull(source);
            return MapInternal<TDest>(source);
        }

        public IEnumerable<TDest> Map<TDest>(IEnumerable<object> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var item in source)
            {
                yield return Map<TDest>(item);
            }
        }

        private TDest MapInternal<TDest>(object source)
        {
            var sourceType = source.GetType();
            var targetType = typeof(TDest);

            var config = MappingConfigurationStore.ReadMappingConfiguration(sourceType, targetType)
                         ?? throw new InvalidOperationException($"No mapping configuration found for {sourceType.FullName} -> {targetType.FullName}");

            var destination = (TDest)PropertyHelper.CreateInstance(targetType);

            var sourceProps = PropertyHelper.BuildPropertyMap(sourceType);
            var targetProps = PropertyHelper.GetPublicReadableWritableProperties(targetType);

            var ignored = config.IgnoredProperties ?? new HashSet<string>();
            var overridden = config.CustomMappingOptions.Select(x => x.TargetProperty.Name).ToHashSet();

            // Step 1: Auto-map matching properties
            foreach (var (sourceProp, targetProp) in PropertyHelper.FindMatchingProperties(sourceProps, targetProps, ignored, overridden))
            {
                var value = sourceProp.GetValue(source);
                SetValueSafely(destination, targetProp, value, sourceProp.PropertyType);
            }

            // Step 2: Apply custom mappings
            foreach (var custom in config.CustomMappingOptions)
            {
                var compiled = custom.SourceExpression.Compile();
                var value = compiled.DynamicInvoke(source);

                SetValueSafely(destination, custom.TargetProperty, value);
            }

            return destination;
        }

        private void SetValueSafely<TDest>(TDest destination, PropertyInfo targetProp, object? value, Type? sourceType = null)
        {
            if (value == null)
            {
                return;
            }

            var fromType = sourceType ?? value.GetType();

            object? finalValue;

            if (PropertyHelper.ShouldMapAsComplexType(fromType, targetProp.PropertyType))
            {
                finalValue = MapComplexType(value, targetProp.PropertyType);
            }
            else
            {
                finalValue = PropertyHelper.ConvertValue(value, targetProp.PropertyType);
            }

            if (targetProp.CanWrite)
            {
                targetProp.SetValue(destination, finalValue);
            }
            else
            {
                throw new InvalidOperationException($"Property {targetProp.Name} on {typeof(TDest).Name} does not have a setter.");
            }
        }


        private object MapComplexType(object value, Type targetType)
        {
            var method = typeof(DefaultMapResolver)
                .GetMethod(nameof(MapInternal), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(targetType);

            return method.Invoke(this, new[] { value })!;
        }
    }
}

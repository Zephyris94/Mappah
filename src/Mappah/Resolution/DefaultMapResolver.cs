namespace Mappah.Resolution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Mappah.Configuration;
    using Mappah.Util;

    public sealed class DefaultMapResolver : IMapResolver
    {
        public TDest Map<TDest, TSource>(TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var sourceType = typeof(TSource);
            var targetType = typeof(TDest);

            var config = MappingConfigurationStore.ReadMappingConfiguration(sourceType, targetType)
                         ?? throw new InvalidOperationException($"No mapping configuration found for {sourceType.FullName} -> {targetType.FullName}");

            var destination = (TDest)PropertyHelper.CreateInstance(targetType);

            var sourceProps = PropertyHelper.BuildPropertyMap(sourceType);
            var targetProps = PropertyHelper.GetPublicReadableWritableProperties(targetType);

            var ignored = config.IgnoredProperties ?? new HashSet<string>();
            var overridden = config.CustomMappingOptions.Select(x => x.TargetProperty.Name).ToHashSet();

            // Step 1: Auto-map matching properties (excluding ignored and overridden)
            foreach (var (sourceProp, targetProp) in PropertyHelper.FindMatchingProperties(sourceProps, targetProps, ignored, overridden))
            {
                var value = sourceProp.GetValue(source);
                var converted = PropertyHelper.ConvertValue(value, targetProp.PropertyType);
                targetProp.SetValue(destination, converted);
            }

            // Step 2: Apply custom mapping rules
            foreach (var custom in config.CustomMappingOptions)
            {
                var compiled = custom.SourceExpression.Compile();
                var value = compiled.DynamicInvoke(source);
                var converted = PropertyHelper.ConvertValue(value, custom.TargetProperty.PropertyType);
                custom.TargetProperty.SetValue(destination, converted);
            }

            return destination;
        }

        public IEnumerable<TDest> Map<TDest, TSource>(IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var item in source)
            {
                yield return Map<TDest, TSource>(item);
            }
        }
    }
}

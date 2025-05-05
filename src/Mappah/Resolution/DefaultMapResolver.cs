using Mappah.Configuration;
using Mappah.Util;
using System.Collections;

namespace Mappah.Resolution
{
    public sealed class DefaultMapResolver : IMapResolver
    {
        public static readonly DefaultMapResolver Instance = new();

        public TDest Map<TDest, TSource>(TSource source)
        {
            return Map<TDest>(source!);
        }
        
        internal TDest MapDefault<TDest>(object source)
        {
            return Map<TDest>(source);
        }

        public TDest Map<TDest>(object source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sourceType = source.GetType();
            var targetType = typeof(TDest);

            var config = MappingConfigurationStore.TryReadMappingConfiguration(sourceType, targetType);
            if (config != null)
            {
                return MapInternalSingleElement<TDest>(source);
            }

            if (source is IEnumerable sourceEnumerable && !(source is string))
            {
                var elementType = PropertyHelper.GetElementType(targetType)
                    ?? throw new InvalidOperationException("Couldn't determine collection element type");

                var mapMethod = typeof(DefaultMapResolver).GetMethod(nameof(Map), new[] { typeof(object) })!;
                var genericMapMethod = mapMethod.MakeGenericMethod(elementType);

                if (targetType.IsArray)
                {
                    var tempList = new List<object>();
                    foreach (var item in sourceEnumerable)
                    {
                        var mapped = genericMapMethod.Invoke(this, new[] { item });
                        tempList.Add(mapped);
                    }

                    var array = Array.CreateInstance(elementType, tempList.Count);
                    for (int i = 0; i < tempList.Count; i++)
                        array.SetValue(tempList[i], i);

                    return (TDest)(object)array;
                }

                var destination = Activator.CreateInstance(targetType)
                                 ?? throw new InvalidOperationException($"Cannot create an instance of type {targetType.FullName}");

                if (destination is not IList destList)
                    throw new InvalidOperationException($"Target type {targetType} must implement IList");

                foreach (var item in sourceEnumerable)
                {
                    var mapped = genericMapMethod.Invoke(this, new[] { item });
                    destList.Add(mapped);
                }

                return (TDest)destination;
            }

            throw new InvalidOperationException($"No mapping configuration found for {source.GetType().FullName} -> {typeof(TDest).FullName}");
        }

        private TDest MapInternalSingleElement<TDest>(object source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var config = MappingConfigurationStore.ReadMappingConfiguration(source.GetType(), typeof(TDest))
                         ?? throw new InvalidOperationException($"No mapping configuration found for {source.GetType().FullName} -> {typeof(TDest).FullName}");

            var destination = Activator.CreateInstance<TDest>()
                             ?? throw new InvalidOperationException($"Cannot create an instance of type {typeof(TDest).FullName}");

            foreach (var mapAction in config.MappingExpressions)
            {
                mapAction(source, destination!);
            }

            return destination;
        }
    }
}

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

            if (source is IEnumerable sourceEnumerable && !(source is string))
            {
                var sourceElementType = PropertyHelper.GetElementType(source.GetType());
                var targetElementType = PropertyHelper.GetElementType(typeof(TDest));

                if (sourceElementType == null || targetElementType == null)
                    throw new InvalidOperationException("Couldn't define collection types");

                var mapMethod = typeof(DefaultMapResolver).GetMethod(nameof(Map), new[] { typeof(object) });
                var genericMapMethod = mapMethod.MakeGenericMethod(targetElementType);

                var destination = Activator.CreateInstance(typeof(TDest));
                if (destination is not IList destinationList)
                    throw new InvalidOperationException("Target collection should inherit IList");

                foreach (var item in sourceEnumerable)
                {
                    var mappedItem = genericMapMethod.Invoke(this, new[] { item });
                    destinationList.Add(mappedItem);
                }

                return (TDest)destination;
            }
            else
            {
                return MapInternalSingleElement<TDest>(source);
            }
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

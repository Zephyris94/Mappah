using Mappah.Configuration;

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

        public IEnumerable<TDest> Map<TDest, TSource>(IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var item in source)
            {
                yield return Map<TDest>(item!);
            }
        }

        public TDest Map<TDest>(object source)
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

        public IEnumerable<TDest> Map<TDest>(IEnumerable<object> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var item in source)
            {
                yield return Map<TDest>(item!);
            }
        }
    }
}

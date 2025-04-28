namespace Mappah.Configuration
{
    using System.Collections.Generic;
    using Mappah.Builder;

    public static class MapperConfiguration
    {
        public static MapBuilder<TSource, TDestination> Create<TSource, TDestination>()
        {
            var source = typeof(TSource);
            var target = typeof(TDestination);

            var config = new List<Action<object, object>>();

            var builder = new MapBuilder<TSource, TDestination>(config);

            MappingConfigurationStore.AddMappingConfiguration((source, target), config);

            return builder;
        }
    }
}

namespace Mappah.Configuration
{
    using System.Collections.Generic;
    using Mappah.Builder;

    internal static class MapperConfiguration
    {
        public static MapBuilder<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            var config = new MappingConfigurationEntity
            {
                Source = typeof(TSource),
                Target = typeof(TDestination),
                CustomMappingOptions = new List<CustomMappingConfigurationOption>()
            };

            var builder = new MapBuilder<TSource, TDestination>(config);

            MappingConfigurationStore.AddMappingConfiguration(config);

            return builder;
        }
    }
}

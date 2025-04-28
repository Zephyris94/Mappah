namespace Mappah.Configuration
{
    using System.Collections.Generic;
    using Mappah.Builder;

    public static class MapperConfigurationBuilder
    {
        private static readonly List<IMapBuilder> _builders = new();

        public static MapBuilder<TSource, TDestination> Create<TSource, TDestination>()
        {
            var entity = new MappingConfigurationEntity();
            var builder = new MapBuilder<TSource, TDestination>(entity);
            _builders.Add(builder);
            return builder;
        }

        public static void Build()
        {
            foreach (var builder in _builders)
            {
                builder.Build();
            }
            _builders.Clear();
        }
    }
}

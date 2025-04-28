namespace Mappah.Configuration
{
    internal sealed class MappingConfigurationStore
    {
        private static readonly Dictionary<(Type Source, Type Target), MappingConfigurationEntity> _mappingConfigurationList
            = new Dictionary<(Type Source, Type Target), MappingConfigurationEntity>();

        public static void AddMappingConfiguration((Type Source, Type Target) key, MappingConfigurationEntity config)
        {
            _mappingConfigurationList.TryAdd(key, config);
        }

        public static MappingConfigurationEntity? ReadMappingConfiguration(Type sourceType, Type targetType)
        {
            if (_mappingConfigurationList.TryGetValue((sourceType, targetType), out var mappingConfiguration))
            {
                return mappingConfiguration;
            }

            throw new Exception($"Mapping configuration of source '{sourceType}' and target '{targetType}' was not found");
        }

    }
}

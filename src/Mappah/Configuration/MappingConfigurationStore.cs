namespace Mappah.Configuration
{
    internal static class MappingConfigurationStore
    {
        private static readonly Dictionary<(Type Source, Type Target), MappingConfigurationEntity> _mappingConfigurations = new();

        public static void AddMappingConfiguration((Type Source, Type Target) key, MappingConfigurationEntity config)
        {
            _mappingConfigurations.TryAdd(key, config);
        }

        public static MappingConfigurationEntity? ReadMappingConfiguration(Type sourceType, Type targetType)
        {
            if (_mappingConfigurations.TryGetValue((sourceType, targetType), out var config))
            {
                return config;
            }

            throw new Exception($"Mapping configuration of source '{sourceType}' and target '{targetType}' was not found");
        }

        public static MappingConfigurationEntity? TryReadMappingConfiguration(Type sourceType, Type targetType)
        {
            if (_mappingConfigurations.TryGetValue((sourceType, targetType), out var config))
            {
                return config;
            }

            return null;
        }
    }
}

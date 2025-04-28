namespace Mappah.Configuration
{
    internal static class MappingConfigurationStore
    {
        private static readonly Dictionary<(Type, Type), List<Action<object, object>>> _mappingConfigurations = new();

        public static void AddMappingConfiguration((Type Source, Type Target) key, List<Action<object, object>> config)
        {
            _mappingConfigurations.TryAdd(key, config);
        }

        public static List<Action<object, object>>? ReadMappingConfiguration(Type sourceType, Type targetType)
        {
            if (_mappingConfigurations.TryGetValue((sourceType, targetType), out var config))
            {
                return config;
            }

            throw new Exception($"Mapping configuration of source '{sourceType}' and target '{targetType}' was not found");
        }
    }
}

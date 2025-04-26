namespace Mappah.Configuration
{
    internal sealed class MappingConfigurationStore
    {
        private static MappingConfigurationEntity[] _mappingConfigurationList = new MappingConfigurationEntity[0];

        public static void AddMappingConfiguration(MappingConfigurationEntity config)
        {
            Array.Resize(ref _mappingConfigurationList, _mappingConfigurationList.Length + 1);

            _mappingConfigurationList[_mappingConfigurationList.Length - 1] = config;
        }

        public static MappingConfigurationEntity? ReadMappingConfiguration(Type sourceType, Type targetType)
        {
            var mappingConfiguration = _mappingConfigurationList.FirstOrDefault(x => x.Source.Equals(sourceType) && x.Target.Equals(targetType));

            return mappingConfiguration;
        }
    }
}

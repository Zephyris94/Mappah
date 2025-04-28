namespace Mappah.Configuration
{
    internal sealed class MappingConfigurationEntity
    {
        public List<CustomMappingConfigurationOption> CustomMappingOptions { get; set; } = new();

        public HashSet<string> IgnoredProperties { get; set; } = new();

    }
}

namespace Mappah.Configuration
{
    internal sealed class MappingConfigurationEntity
    {
        public Type Source { get; set; }

        public Type Target { get; set; }

        public List<CustomMappingConfigurationOption> CustomMappingOptions { get; set; } = new();

        public HashSet<string> IgnoredProperties { get; set; } = new();

    }
}

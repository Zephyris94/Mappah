using System.Linq.Expressions;

namespace Mappah.Configuration
{
    public sealed class MappingConfigurationEntity
    {
        public Dictionary<string, LambdaExpression> ManualMappings { get; set; } = new();
        public HashSet<string> SkippedProperties { get; set; } = new();
        public List<Action<object, object>> MappingExpressions { get; set; } = new();
    }

}

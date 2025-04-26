namespace Mappah
{
    using System.Linq.Expressions;
    using System.Reflection;

    public sealed class CustomMappingConfigurationOption
    {
        public PropertyInfo TargetProperty { get; set; }
        public LambdaExpression SourceExpression { get; set; }
    }
}

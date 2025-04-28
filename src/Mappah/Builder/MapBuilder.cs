using Mappah.Configuration;
using Mappah.Util;
using System.Linq.Expressions;
using System.Reflection;

namespace Mappah.Builder
{
    public sealed class MapBuilder<TSource, TDestination> : IMapBuilder
    {
        private readonly MappingConfigurationEntity _config;

        public MapBuilder(MappingConfigurationEntity config)
        {
            _config = config;
        }

        public MapBuilder<TSource, TDestination> For<TDestMember>(Expression<Func<TDestination, TDestMember>> targetMember, Expression<Func<TSource, object>> sourceExpression)
        {
            var targetName = GetMemberName(targetMember);
            _config.ManualMappings[targetName] = sourceExpression;
            return this;
        }

        public MapBuilder<TSource, TDestination> Skip<TDestMember>(Expression<Func<TDestination, TDestMember>> targetMember)
        {
            var targetName = GetMemberName(targetMember);
            _config.SkippedProperties.Add(targetName);
            return this;
        }

        public void Build()
        {
            foreach (var targetProp in typeof(TDestination).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!targetProp.CanWrite || !targetProp.CanRead)
                    continue;

                if (_config.SkippedProperties.Contains(targetProp.Name))
                    continue;

                if (_config.ManualMappings.TryGetValue(targetProp.Name, out var manualSource))
                {
                    var action = MappingExpressionCompiler.CompileManual<TSource, TDestination>(manualSource, targetProp);
                    _config.MappingExpressions.Add(action);
                }
                else
                {
                    var sourceProp = typeof(TSource).GetProperty(targetProp.Name);
                    if (sourceProp == null || !sourceProp.CanRead)
                        continue;

                    var action = MappingExpressionCompiler.CompileAuto<TSource, TDestination>(sourceProp, targetProp);
                    _config.MappingExpressions.Add(action);
                }
            }

            _config.SkippedProperties.Clear();
            _config.ManualMappings.Clear();

            MappingConfigurationStore.AddMappingConfiguration((typeof(TSource), typeof(TDestination)), _config);
        }

        private static string GetMemberName<T, TProp>(Expression<Func<T, TProp>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression member)
                return member.Member.Name;

            if (memberSelector.Body is UnaryExpression unary && unary.Operand is MemberExpression memberOperand)
                return memberOperand.Member.Name;

            throw new ArgumentException("Invalid expression");
        }
    }

}

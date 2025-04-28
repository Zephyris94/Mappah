using Mappah.Configuration;
using Mappah.Util;
using System.Linq.Expressions;
using System.Reflection;

namespace Mappah.Builder
{
    public sealed class MapBuilder<TSource, TDestination>
    {
        private readonly List<Action<object, object>> _mappingExpressions;
        private readonly Dictionary<string, LambdaExpression> _manualMappings = new();
        private readonly HashSet<string> _skippedProperties = new();

        public MapBuilder(List<Action<object, object>> mappingExpressions)
        {
            _mappingExpressions = mappingExpressions;
        }

        public MapBuilder<TSource, TDestination> For<TDestMember, TSourceMember>(
            Expression<Func<TDestination, TDestMember>> targetMember,
            Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var targetName = GetMemberName(targetMember);
            _manualMappings[targetName] = sourceMember;
            return this;
        }

        public MapBuilder<TSource, TDestination> Skip<TDestMember>(
            Expression<Func<TDestination, TDestMember>> targetMember)
        {
            var targetName = GetMemberName(targetMember);
            _skippedProperties.Add(targetName);
            return this;
        }

        public void Build()
        {
            foreach (var targetProp in typeof(TDestination).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!targetProp.CanWrite || !targetProp.CanRead)
                    continue;

                if (_skippedProperties.Contains(targetProp.Name))
                    continue;

                if (_manualMappings.TryGetValue(targetProp.Name, out var manualSource))
                {
                    var action = MappingExpressionCompiler.CompileManual<TSource, TDestination>(manualSource, targetProp);
                    _mappingExpressions.Add(action);
                }
                else
                {
                    var sourceProp = typeof(TSource).GetProperty(targetProp.Name, BindingFlags.Instance | BindingFlags.Public);

                    if (sourceProp != null && sourceProp.CanRead)
                    {
                        var action = MappingExpressionCompiler.CompileAuto<TSource, TDestination>(sourceProp, targetProp);
                        _mappingExpressions.Add(action);
                    }
                }
            }
        }

        private static string GetMemberName<T, TMember>(Expression<Func<T, TMember>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpr)
                throw new InvalidOperationException("Only simple property accessors are allowed.");

            return memberExpr.Member.Name;
        }
    }
}

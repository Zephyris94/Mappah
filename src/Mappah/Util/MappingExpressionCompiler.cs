using Mappah.Resolution;
using System.Linq.Expressions;
using System.Reflection;

namespace Mappah.Util
{
    internal static class MappingExpressionCompiler
    {
        public static Action<object, object> CompileManual<TSource, TDestination>(
            LambdaExpression sourceExpression,
            PropertyInfo targetProperty)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var destParam = Expression.Parameter(typeof(object), "destination");

            var sourceCast = Expression.Convert(sourceParam, typeof(TSource));
            var destCast = Expression.Convert(destParam, typeof(TDestination));

            var invokeSource = Expression.Invoke(sourceExpression, sourceCast);
            var destProp = Expression.Property(destCast, targetProperty);

            return CompileCore(invokeSource, destProp, sourceParam, destParam);
        }

        public static Action<object, object> CompileAuto<TSource, TDestination>(
            PropertyInfo sourceProperty,
            PropertyInfo targetProperty)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var destParam = Expression.Parameter(typeof(object), "destination");

            var sourceCast = Expression.Convert(sourceParam, typeof(TSource));
            var destCast = Expression.Convert(destParam, typeof(TDestination));

            var sourceValue = Expression.Property(sourceCast, sourceProperty);
            var destValue = Expression.Property(destCast, targetProperty);

            return CompileCore(sourceValue, destValue, sourceParam, destParam);
        }

        private static Action<object, object> CompileCore(
            Expression sourceValue,
            Expression destValue,
            ParameterExpression sourceParam,
            ParameterExpression destParam)
        {
            Expression valueToAssign;

            if (PropertyHelper.ShouldMapAsComplexType(sourceValue.Type, destValue.Type))
            {
                var mapMethod = typeof(DefaultMapResolver)
                    .GetMethod(nameof(DefaultMapResolver.MapDefault), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(destValue.Type);

                valueToAssign = Expression.Call(
                    Expression.Constant(DefaultMapResolver.Instance),
                    mapMethod,
                    sourceValue
                );
            }
            else
            {
                valueToAssign = Expression.Convert(sourceValue, destValue.Type);
            }

            var assign = Expression.Assign(destValue, valueToAssign);

            var block = Expression.Block(assign);

            return Expression.Lambda<Action<object, object>>(block, sourceParam, destParam).Compile();
        }
    }
}

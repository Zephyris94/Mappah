using Mappah.Resolution;
using System.Collections;
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

        public static Action<object, object> CompileCollection<TSource, TDestination, TSourceItem, TDestItem>(
            PropertyInfo sourceProp, PropertyInfo targetProp)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var destParam = Expression.Parameter(typeof(object), "destination");

            var sourceCast = Expression.Convert(sourceParam, typeof(TSource));
            var destCast = Expression.Convert(destParam, typeof(TDestination));

            var sourceValue = Expression.Property(sourceCast, sourceProp);
            var destValue = Expression.Property(destCast, targetProp);

            // typeof(List<DestItem>)
            var targetCollectionType = targetProp.PropertyType;
            if (!typeof(IList).IsAssignableFrom(targetCollectionType))
                throw new InvalidOperationException($"Target property '{targetProp.Name}' must implement IList.");

            // Create List<DestItem>
            var listCtor = Expression.New(targetCollectionType);
            var listVar = Expression.Variable(targetCollectionType, "list");

            var assignList = Expression.Assign(destValue, listVar);

            // Foreach loop over sourceValue (IEnumerable<TSourceItem>)
            var elementVar = Expression.Variable(typeof(TSourceItem), "item");
            var loopVar = Expression.Variable(typeof(IEnumerator<TSourceItem>), "enumerator");
            var breakLabel = Expression.Label("LoopBreak");

            var getEnumeratorCall = Expression.Call(sourceValue, typeof(IEnumerable<TSourceItem>).GetMethod("GetEnumerator")!);
            var assignEnumerator = Expression.Assign(loopVar, getEnumeratorCall);

            var moveNextCall = Expression.Call(loopVar, typeof(IEnumerator).GetMethod("MoveNext")!);
            var loopBody = Expression.Block(
                new[] { elementVar },
                Expression.Assign(elementVar, Expression.Property(loopVar, "Current")),

                // map item
                Expression.Call(
                    listVar,
                    targetCollectionType.GetMethod("Add")!,
                    Expression.Call(
                        Expression.Constant(DefaultMapResolver.Instance),
                        typeof(DefaultMapResolver).GetMethod("MapDefault", BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(typeof(TDestItem)),
                        Expression.Convert(elementVar, typeof(object))
                    )
                )
            );

            var loop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.IsTrue(moveNextCall),
                    loopBody,
                    Expression.Break(breakLabel)
                ),
                breakLabel
            );

            var block = Expression.Block(
                new[] { listVar, loopVar },
                Expression.Assign(listVar, listCtor),
                Expression.Assign(destValue, listVar),
                assignEnumerator,
                loop
            );

            return Expression.Lambda<Action<object, object>>(block, sourceParam, destParam).Compile();
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

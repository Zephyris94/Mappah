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
            PropertyInfo sourceProp,
            PropertyInfo targetProp)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var destParam = Expression.Parameter(typeof(object), "destination");

            var sourceCast = Expression.Convert(sourceParam, typeof(TSource));
            var destCast = Expression.Convert(destParam, typeof(TDestination));

            // sourceCollection = ((TSource)source).<sourceProp>
            var sourceCollection = Expression.Property(sourceCast, sourceProp);

            // list = new List<TDestItem>();
            var listVar = Expression.Variable(typeof(List<TDestItem>), "list");
            var listCtor = Expression.New(typeof(List<TDestItem>));
            var assignList = Expression.Assign(listVar, listCtor);

            // foreach (var item in sourceCollection)
            var itemVar = Expression.Variable(typeof(TSourceItem), "item");
            var enumeratorVar = Expression.Variable(typeof(IEnumerator<TSourceItem>), "enumerator");

            var getEnumerator = Expression.Call(sourceCollection, typeof(IEnumerable<TSourceItem>).GetMethod("GetEnumerator")!);
            var moveNext = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext")!);
            var current = Expression.Property(enumeratorVar, "Current");
            var breakLabel = Expression.Label("LoopBreak");

            var mapMethod = typeof(DefaultMapResolver)
                .GetMethod(nameof(DefaultMapResolver.MapDefault), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(typeof(TDestItem));

            var mappedItem = Expression.Call(Expression.Constant(DefaultMapResolver.Instance), mapMethod, itemVar);
            var addCall = Expression.Call(listVar, typeof(List<TDestItem>).GetMethod("Add")!, mappedItem);

            var loopBody = Expression.Block(
                new[] { enumeratorVar, itemVar },
                Expression.Assign(enumeratorVar, getEnumerator),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.IsFalse(moveNext),
                        Expression.Break(breakLabel),
                        Expression.Block(
                            Expression.Assign(itemVar, current),
                            addCall
                        )
                    ),
                    breakLabel
                )
            );

            // Преобразование листа в нужный тип (array или просто лист)
            Expression assignedCollection;

            if (targetProp.PropertyType.IsArray)
            {
                var toArrayCall = Expression.Call(listVar, typeof(List<TDestItem>).GetMethod("ToArray")!);
                assignedCollection = Expression.Assign(Expression.Property(destCast, targetProp), toArrayCall);
            }
            else
            {
                assignedCollection = Expression.Assign(Expression.Property(destCast, targetProp), listVar);
            }

            var body = Expression.Block(
                new[] { listVar },
                assignList,
                loopBody,
                assignedCollection
            );

            return Expression.Lambda<Action<object, object>>(body, sourceParam, destParam).Compile();
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

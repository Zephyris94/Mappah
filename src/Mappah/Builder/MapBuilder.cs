namespace Mappah.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Mappah.Configuration;

    public sealed class MapBuilder<TSource, TDestination>
    {
        private readonly MappingConfigurationEntity _config;

        internal MapBuilder(MappingConfigurationEntity config)
        {
            _config = config;
        }

        /// <summary>
        /// Defines a mapping between a destination property and a source expression.
        /// </summary>
        /// <typeparam name="TMember">The type of the mapped member (property type).</typeparam>
        /// <param name="targetMember">Expression representing the destination property.</param>
        /// <param name="sourceMember">Expression representing the source property or calculated value.</param>
        /// <returns>The current <see cref="MapBuilder{TSource, TDestination}"/> instance for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the destination expression is not a simple property access.</exception>
        public MapBuilder<TSource, TDestination> For<TTargetMember, TSourceMember>(
            Expression<Func<TDestination, TTargetMember>> targetMember,
            Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            if (targetMember.Body is not MemberExpression memberExpression)
            {
                throw new InvalidOperationException("'For' expects a simple property access on the destination.");
            }

            var targetProperty = (PropertyInfo)memberExpression.Member;

            _config.CustomMappingOptions.Add(new CustomMappingConfigurationOption
            {
                TargetProperty = targetProperty,
                SourceExpression = sourceMember
            });

            return this;
        }

        /// <summary>
        /// Marks the specified destination property to be ignored during mapping.
        /// </summary>
        /// <typeparam name="TMember">The type of the destination property.</typeparam>
        /// <param name="targetMember">Expression representing the destination property to ignore.</param>
        /// <returns>The current <see cref="MapBuilder{TSource, TDestination}"/> instance for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the destination expression is not a simple property access.</exception>
        public MapBuilder<TSource, TDestination> Skip<TMember>(
            Expression<Func<TDestination, TMember>> targetMember)
        {
            if (targetMember.Body is not MemberExpression memberExpression)
            {
                throw new InvalidOperationException("'Skip' expects a simple property access on the destination.");
            }

            var targetProperty = (PropertyInfo)memberExpression.Member;

            _config.IgnoredProperties.Add(targetProperty.Name);

            return this;
        }

        /// <summary>
        /// Indicates presence of reverse mapping
        /// </summary>
        public void WithReverse()
        {
            var reverseConfig = new MappingConfigurationEntity
            {
                Source = typeof(TDestination),
                Target = typeof(TSource),
                CustomMappingOptions = new List<CustomMappingConfigurationOption>(),
                IgnoredProperties = new HashSet<string>()
            };

            // Reverse mapping transition
            foreach (var option in _config.CustomMappingOptions)
            {
                if (option.SourceExpression is LambdaExpression sourceExpr &&
                    sourceExpr.Body is MemberExpression sourceMemberExpr)
                {
                    var sourceProp = (PropertyInfo)sourceMemberExpr.Member;

                    reverseConfig.CustomMappingOptions.Add(new CustomMappingConfigurationOption
                    {
                        TargetProperty = sourceProp,
                        SourceExpression = Expression.Lambda
                        (Expression.Property
                        (Expression.Parameter(typeof(TDestination), "src")
                        , option.TargetProperty)
                        , Expression.Parameter(typeof(TDestination), "src"))
                    });
                }
            }

            // Ignored properties transition
            foreach (var ignoredProp in _config.IgnoredProperties)
            {
                reverseConfig.IgnoredProperties.Add(ignoredProp);
            }

            MappingConfigurationStore.AddMappingConfiguration(reverseConfig);
        }
    }
}

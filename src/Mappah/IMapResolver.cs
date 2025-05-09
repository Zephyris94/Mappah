﻿namespace Mappah
{
    public interface IMapResolver
    {
        /// <summary>
        /// Maps a source object to a new instance of the destination type.
        /// </summary>
        /// <typeparam name="TDest">The type of the destination object.</typeparam>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>A new instance of the destination type with mapped properties.</returns>
        TDest Map<TDest, TSource>(TSource source);

        /// <summary>
        /// Maps a collection of source objects to a collection of destination objects.
        /// </summary>
        /// <typeparam name="TDest">The type of the destination objects.</typeparam>
        /// <typeparam name="TSource">The type of the source objects.</typeparam>
        /// <param name="source">The collection of source objects.</param>
        /// <returns>A collection of mapped destination objects.</returns>
        IEnumerable<TDest> Map<TDest, TSource>(IEnumerable<TSource> source);

        /// <summary>
        /// Maps a source object (typed as object) to a new instance of the destination type.
        /// Intended for dynamic scenarios when the source type is unknown at compile time.
        /// </summary>
        /// <typeparam name="TDest">The type of the destination object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>A new instance of the destination type with mapped properties.</returns>
        TDest Map<TDest>(object source);

        /// <summary>
        /// Maps a collection of source objects (typed as object) to a collection of destination objects.
        /// Intended for dynamic scenarios when the source types are unknown at compile time.
        /// </summary>
        /// <typeparam name="TDest">The type of the destination objects.</typeparam>
        /// <param name="source">The collection of source objects.</param>
        /// <returns>A collection of mapped destination objects.</returns>
        IEnumerable<TDest> Map<TDest>(IEnumerable<object> source);
    }
}

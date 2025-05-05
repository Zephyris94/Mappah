namespace Mappah
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
        /// Maps a source object (typed as object) to a new instance of the destination type.
        /// Intended for dynamic scenarios when the source type is unknown at compile time.
        /// </summary>
        /// <typeparam name="TDest">The type of the destination object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>A new instance of the destination type with mapped properties.</returns>
        TDest Map<TDest>(object source);
    }
}

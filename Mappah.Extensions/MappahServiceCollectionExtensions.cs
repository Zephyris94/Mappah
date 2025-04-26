using Mappah.Resolution;
using Microsoft.Extensions.DependencyInjection;

namespace Mappah.Extensions
{
    public static class MappahServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Mappah IMapResolver implementation to the service collection.
        /// </summary>
        public static IServiceCollection AddMappah(this IServiceCollection services)
        {
            services.AddSingleton<IMapResolver, DefaultMapResolver>();
            return services;
        }
    }
}

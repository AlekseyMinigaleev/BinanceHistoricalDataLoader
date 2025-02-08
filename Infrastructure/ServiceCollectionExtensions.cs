using Hangfire;
using Infrastructure.Hangfire;
using Infrastructure.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            services.AddMongoDb(configuration);
            services.AddHangfire(configuration);

            return services;
        }
    }
}
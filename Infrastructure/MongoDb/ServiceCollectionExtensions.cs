using Infrastructure.Extensions;
using Infrastructure.MongoDb.ConfigurationModels;
using Infrastructure.MongoDb.Services.MongoDbInitializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure.MongoDb
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDb(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            var connectionString = configuration
                .GetConnectionString("MongoDb");
            var mongoDbConfiguration = configuration
               .GetConfigurationModel<MongoDbConfiguration>("MongoDbConfiguration");

            services.AddSingleton(new MongoClient(connectionString));
            services.AddIMongoDatabase(mongoDbConfiguration);

            services.AddTransient<IMongoDbInitializer, MongoDbInitializer>();

            return services;
        }

        private static IServiceCollection AddIMongoDatabase(
          this IServiceCollection services,
          MongoDbConfiguration mongoDbConfiguration)
        {
            services.AddScoped(provider =>
            {
                var client = provider.GetRequiredService<MongoClient>();
                var db = client.GetDatabase(mongoDbConfiguration.DatabaseName);
                return db;
            });

            return services;
        }
    }
}
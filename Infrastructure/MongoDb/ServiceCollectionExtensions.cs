using Infrastructure.Extensions;
using Infrastructure.MongoDb.ConfigurationModels;
using Infrastructure.MongoDb.Services.MongoDbInitializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
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

            services.AddIMongoDatabase(mongoDbConfiguration);
            services.AddMongoClient(connectionString);

            services.AddTransient<IMongoDbInitializer, MongoDbInitializer>();

            return services;
        }

        private static IServiceCollection AddMongoClient(
            this IServiceCollection services,
            string? connectionString)
        {
            var mongoClientSettings = MongoClientSettings.FromUrl(
                new MongoUrl(connectionString));

            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            services.AddSingleton(new MongoClient(mongoClientSettings));

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
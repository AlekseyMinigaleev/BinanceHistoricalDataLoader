using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Infrastructure.Extensions;
using Infrastructure.Hangfire.ConfigurationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Hangfire
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfire(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration
                .GetConnectionString("Hangfire");
            var hangfireConfiguration = configuration
                .GetConfigurationModel<HangfireConfiguration>("HangfireConfiguration");

            services.AddHangfire(connectionString, hangfireConfiguration);
            services.AddHangfireServer();

            return services;
        }

        private static IServiceCollection AddHangfire(
            this IServiceCollection services,
            string? connectionString,
            HangfireConfiguration hangfireConfiguration)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel((CompatibilityLevel)hangfireConfiguration.CompatibilityLevel)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(
                    connectionString,
                    hangfireConfiguration.DatabaseName,
                    new MongoStorageOptions
                    {
                        MigrationOptions = new MongoMigrationOptions
                        {
                            MigrationStrategy = new MigrateMongoMigrationStrategy(),
                            BackupStrategy = new CollectionMongoBackupStrategy()
                        },
                        Prefix = hangfireConfiguration.Prefix,
                        CheckConnection = true,
                        CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.Poll
                    }));

            return services;
        }
    }
}
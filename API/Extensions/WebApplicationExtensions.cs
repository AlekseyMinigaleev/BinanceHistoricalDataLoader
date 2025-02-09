using Infrastructure.MongoDb.Services.MongoDbInitializer;
using Serilog;

namespace API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task InitApp(
            this WebApplication app)
        {
            Log.Information("Starting application initialization.");

            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            Log.Information("Initializing database");
            await InitMongoDbAsync(serviceProvider);
            Log.Information("Database initialization complete.");

            Log.Information("Application initialization finished successfully.");
        }

        private static async Task InitMongoDbAsync(
           IServiceProvider serviceProvider,
           CancellationToken cancellationToken = default)
        {
            var mongoDbInitializer = serviceProvider.GetRequiredService<IMongoDbInitializer>();
            await mongoDbInitializer.InitAsync(cancellationToken);
        }
    }
}

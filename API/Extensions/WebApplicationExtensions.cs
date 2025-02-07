using Infrastructure.MongoDb.Services.MongoDbInitializer;

namespace API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task InitApp(
            this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            await InitMongoDbAsync(serviceProvider);
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

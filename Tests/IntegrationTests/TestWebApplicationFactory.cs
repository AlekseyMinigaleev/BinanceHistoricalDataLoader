using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;

namespace Tests.IntegrationTests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        public MongoDbRunner MongoRunner { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            MongoRunner = MongoDbRunner.Start();

            builder.ConfigureServices(services =>
            {
                var mongoClientSettings = MongoClientSettings.FromUrl(
                    new MongoUrl(MongoRunner.ConnectionString));

                services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoClientSettings));

                services.AddScoped(provider =>
                {
                    var client = provider.GetRequiredService<IMongoClient>();
                    var db = client.GetDatabase("test");
                    return db;
                });

                services.AddHangfire(config => config.UseMemoryStorage());
                services.AddHangfireServer();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            MongoRunner?.Dispose();
        }
    }
}
using Domain.Models.Candlestick;
using Domain.Models.Job;
using Domain.Models.Report;
using MongoDB.Driver;

namespace Infrastructure.MongoDb.Services.MongoDbInitializer
{
    public class MongoDbInitializer(IMongoDatabase db) : IMongoDbInitializer
    {
        private readonly IMongoDatabase _db = db;

        public async Task InitAsync(CancellationToken cancellationToken)
        {
            await CreateCollectionsAsync(cancellationToken);
        }

        private async Task CreateCollectionsAsync(CancellationToken cancellationToken)
        {
            var cursor = await _db.ListCollectionNamesAsync(cancellationToken: cancellationToken);
            var existingCollections = await cursor.ToListAsync(cancellationToken);

            await EnsureCreateCollectionAsync(
                nameof(Job),
                existingCollections,
                cancellationToken);

            await EnsureCreateCollectionAsync(
                nameof(Report),
                existingCollections,
                cancellationToken);

            await EnsureCreateCollectionAsync(
                nameof(Candlestick),
                existingCollections,
                cancellationToken);
        }

        private async Task EnsureCreateCollectionAsync(
            string collectionName,
            List<string> existingCollections,
            CancellationToken cancellationToken)
        {
            if (!existingCollections.Contains(collectionName))
                await _db.CreateCollectionAsync(
                    collectionName,
                    cancellationToken: cancellationToken);
        }
    }
}
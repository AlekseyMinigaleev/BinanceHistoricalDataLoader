namespace Infrastructure.MongoDb.Services.MongoDbInitializer
{
    public interface IMongoDbInitializer
    {
        public Task InitAsync(CancellationToken cancellationToken);
    }
}

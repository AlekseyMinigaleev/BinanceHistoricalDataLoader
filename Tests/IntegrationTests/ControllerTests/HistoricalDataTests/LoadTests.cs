using Domain.Models.Job;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;
using System.Text;
using static API.Controllers.HistoricalData.Actions.Load;

namespace Tests.IntegrationTests.ControllerTests.HistoricalDataTests
{
    public class LoadTests(
        TestWebApplicationFactory app)
        : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _app = app;

        [Fact]
        public async Task Load_Post_Should_CreateJobInDbAndEnqueueHangfireJob()
        {
            var factory = _app.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var backgroundJobClientMock = new Mock<IBackgroundJobClient>();
                    backgroundJobClientMock
                        .Setup(x => x.Create(It.IsAny<Hangfire.Common.Job>(), It.IsAny<IState>()))
                        .Returns("fake-job-id");

                    services.AddSingleton(backgroundJobClientMock.Object);
                    services.AddSingleton(backgroundJobClientMock);
                });
            });

            using var httpClient = factory.CreateClient();

            var requestBody = new
            {
                Pairs = new List<string> { "EURUSD", "USDJPY" },
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/api/historical-data/load", jsonContent);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var loadResponse = JsonConvert.DeserializeObject<LoadResponse>(responseContent);

            Assert.NotNull(loadResponse);
            Assert.NotEqual(Guid.Empty, loadResponse.JobId);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            var jobsCollection = db.GetCollection<Job>(nameof(Job));
            var filter = Builders<Job>.Filter.Eq(x => x.Id, loadResponse.JobId);
            var jobDocument = await jobsCollection.Find(filter).FirstOrDefaultAsync();

            Assert.NotNull(jobDocument);

            var backgroundJobClientMock = scope.ServiceProvider.GetRequiredService<Mock<IBackgroundJobClient>>();
            backgroundJobClientMock.Verify(
                x => x.Create(It.IsAny<Hangfire.Common.Job>(), It.IsAny<IState>()),
                Times.Once,
                "Метод Create должен быть вызван один раз");
        }
    }
}
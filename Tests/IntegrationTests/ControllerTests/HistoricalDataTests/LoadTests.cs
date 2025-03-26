using Domain.Models.Job;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Mvc.Testing;
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
            using var app = CreateApp();
            using var httpClient = app.CreateClient();

            var httpResponseMessage = await SendLoadRequest(httpClient);

            httpResponseMessage.EnsureSuccessStatusCode();
            var loadResponse = await GetLoadResponseAsync(httpResponseMessage);

            VerifyLoadResponse(loadResponse);
            await VerifyJobExistsInDb(app, loadResponse!.JobId);
            VerifyHangfireJobEnqueued(app);
        }

        private WebApplicationFactory<Program> CreateApp()
        {
            var app = _app.WithWebHostBuilder(builder =>
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

            return app;
        }

        private async Task<HttpResponseMessage> SendLoadRequest(HttpClient client)
        {
            var requestBody = new
            {
                Pairs = new List<string> { "EURUSD", "USDJPY" },
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/historical-data/load", jsonContent);

            return response;
        }

        private async Task<LoadResponse?> GetLoadResponseAsync(HttpResponseMessage httpResponseMessage)
        {
            var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            var loadResponse = JsonConvert.DeserializeObject<LoadResponse>(responseContent);

            return loadResponse;
        }

        private void VerifyLoadResponse(LoadResponse? loadResponse)
        {
            Assert.NotNull(loadResponse);
            Assert.NotEqual(Guid.Empty, loadResponse.JobId);
        }

        private async Task VerifyJobExistsInDb(WebApplicationFactory<Program> app, Guid jobId)
        {
            using var scope = app.Services
                .CreateScope();
            var db = scope.ServiceProvider
                .GetRequiredService<IMongoDatabase>();

            var jobsCollection = db.GetCollection<Job>(nameof(Job));

            var filter = Builders<Job>.Filter
                .Eq(x => x.Id, jobId);

            var jobDocument = await jobsCollection
                .Find(filter)
                .FirstOrDefaultAsync();

            Assert.NotNull(jobDocument);
        }

        private void VerifyHangfireJobEnqueued(WebApplicationFactory<Program> app)
        {
            using var scope = app.Services.CreateScope();
            var backgroundJobClientMock = scope.ServiceProvider
                .GetRequiredService<Mock<IBackgroundJobClient>>();

            backgroundJobClientMock.Verify(
                x => x.Create(It.IsAny<Hangfire.Common.Job>(), It.IsAny<IState>()),
                Times.Once,
                "Job was not enqueued");
        }
    }
}
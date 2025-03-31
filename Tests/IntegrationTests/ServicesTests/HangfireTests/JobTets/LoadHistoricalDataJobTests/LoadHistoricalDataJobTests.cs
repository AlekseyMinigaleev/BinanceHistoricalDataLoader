using Domain.Models.Job;
using Domain.Models.Kline;
using Domain.Models.Report;
using FluentAssertions;
using Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Job = Domain.Models.Job.Job;

namespace Tests.IntegrationTests.ServicesTests.HangfireTests.JobTets.LoadHistoricalDataJobTests
{
    public class LoadHistoricalDataJobTests(
        TestWebApplicationFactory app)
        : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _app = app;

        [Fact]
        public async Task LoadHistoricalData_HappyPath_ShouldCreateReportAndKlinesAndUpdateJob()
        {
            //arrange
            using var client = _app.CreateClient();
            using var scope = _app.Services.CreateScope();

            var historicalDataJob = scope.ServiceProvider.GetRequiredService<ILoadHistoricalDataJob>();
            var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var expectedJob = CreateJob();
            await InsertJobToDbAsync(expectedJob, db);

            //act
            await historicalDataJob.LoadHistoricalDataAsync(expectedJob, default, default);

            //assert
            var actualJob = await GetJobFromDbAsync(db, expectedJob.Id);
            VerifyJobIsCorrectlyModified(expectedJob, actualJob);

            var actualReport = await GetReportFromDbAsync(db, actualJob!.ReportId!.Value);
            VerifyReportIsValid(actualReport, actualJob.Parameters);

            await VerifyKlinesValidCountAsync(
                actualJob.Parameters,
                [.. actualReport!.Candlesticks],
                db);
        }

        private Job CreateJob()
        {
            var jobParameters = new JobParameters(
               ["BTCUSDT", "ETHUSDT"],
               DateTime.UtcNow.AddDays(-1),
               DateTime.UtcNow);
            var job = new Job(jobParameters);

            return job;
        }

        private void VerifyJobIsCorrectlyModified(Job expected, Job? actual)
        {
            Assert.NotNull(actual);
            expected.Parameters.Should().BeEquivalentTo(actual.Parameters);
            Assert.NotNull(actual.ReportId);
            Assert.NotEqual(Guid.Empty, actual.ReportId);
            Assert.Null(actual.ErrorMessage);
            Assert.Equal(JobStatus.Completed, actual.Status);
            var emptyDateTime = new DateTime();
            Assert.NotNull(actual.StartTime);
            Assert.NotEqual(emptyDateTime, actual.StartTime);
            Assert.NotNull(actual.EndTime);
            Assert.NotEqual(emptyDateTime, actual.EndTime);
        }

        private void VerifyReportIsValid(
            Report? actualReport,
            JobParameters parameters)
        {
            Assert.NotNull(actualReport);
            Assert.Equal(parameters.Symbols.Count, actualReport.Candlesticks.Count);

            var actualSymbols = actualReport.Candlesticks
                .Select(x => x.Symbol)
                .ToList();

            actualSymbols
                .Should()
                .BeEquivalentTo(parameters.Symbols);
        }

        private async Task VerifyKlinesValidCountAsync(
            JobParameters jobParameters,
            List<Candlestick> actualCandlesticks,
            IMongoDatabase db)
        {
            var expectedKlinesCount = (jobParameters.StartDate - jobParameters.EndDate).Days;

            foreach (var actualCandlestick in actualCandlesticks)
            {
                var actualKlines = await GetKlinesAsync(db, [.. actualCandlestick.KlineIds]);
                Assert.Equal(expectedKlinesCount, actualKlines.Count);
            }
        }

        private async Task InsertJobToDbAsync(Job job, IMongoDatabase db)
        {
            var jobs = db.GetCollection<Job>(nameof(Job));
            await jobs.InsertOneAsync(job);
        }

        private async Task<Job?> GetJobFromDbAsync(
            IMongoDatabase db,
            Guid jobId)
        {
            var jobs = db.GetCollection<Job>(nameof(Job));
            var filter = Builders<Job>.Filter.Eq(x => x.Id, jobId);
            var job = await jobs
                .Find(filter)
                .FirstOrDefaultAsync();

            return job;
        }

        private async Task<Report?> GetReportFromDbAsync(
            IMongoDatabase db,
            Guid reportId)
        {
            var reports = db.GetCollection<Report>(nameof(Report));
            var filter = Builders<Report>.Filter.Eq(x => x.Id, reportId);
            var report = await reports
                .Find(filter)
                .FirstOrDefaultAsync();

            return report;
        }

        private async Task<List<Kline>> GetKlinesAsync(
            IMongoDatabase db,
            List<Guid> klineIds)
        {
            var klines = db.GetCollection<Kline>(nameof(Kline));
            var filter = Builders<Kline>.Filter.In(k => k.Id, klineIds);
            var result = await klines
                .Find(filter)
                .ToListAsync();

            return result;
        }
    }
}


//private async Task<List<CandlestickTestModel>> FetchExpectedCandlesticksAsync(
//            Job expectedJob,
//            IHttpClientFactory httpClientFactory)
//{
//    var httpUtils = new KlinesHttpUtils(httpClientFactory);
//    var expectedCandlesticks = new List<CandlestickTestModel>();
//    foreach (var symbol in expectedJob.Parameters.Symbols)
//    {
//        var parameters = new Parameters(
//            symbol: symbol,
//            startDate: expectedJob.Parameters.StartDate,
//            endDate: expectedJob.Parameters.EndDate,
//            interval: Interval.OneDay);

//        var klines = await httpUtils.FetchDataAsync(parameters, default);

//        expectedCandlesticks.Add(new CandlestickTestModel(parameters, klines));
//    }

//    return expectedCandlesticks;
//}
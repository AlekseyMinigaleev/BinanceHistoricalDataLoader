using Domain.Models.Job;
using Domain.Models.Kline;
using Domain.Models.Report;
using FluentAssertions;
using Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Tests.IntegrationTests.HttpUtils.Klines;
using Tests.IntegrationTests.HttpUtils.Klines.Klines;
using Job = Domain.Models.Job.Job;

namespace Tests.IntegrationTests.ServicesTests.HangfireTests.JobTets.LoadHistoricalDataJobTests
{
    public class LoadHistoricalDataJobTests(
        TestWebApplicationFactory app)
        : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _app = app;

        [Fact]
        public async Task LoadHistoricalDataAsync_HappyPath_ShouldCreateReportAndKlinesAndUpdateJob()
        {
            using var client = _app.CreateClient();
            using var scope = _app.Services.CreateScope();

            var historicalDataJob = scope.ServiceProvider.GetRequiredService<ILoadHistoricalDataJob>();
            var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var httpUtils = new KlinesHttpUtils(
                httpClientFactory);

            var job = CreateJob();
            await InsertJobToDbAsync(job, db);

            var candleSticksTestModel = new List<CandlestickTestModel>();

            foreach (var symbol in job.Parameters.Symbols)
            {
                var parameters = new Parameters(
                    symbol: symbol,
                    startDate: job.Parameters.StartDate,
                    endDate: job.Parameters.EndDate,
                    interval: Interval.OneDay);

                var klines = await httpUtils.FetchDataAsync(parameters, default);

                candleSticksTestModel.Add(new CandlestickTestModel(parameters, klines));
            }

            await historicalDataJob.LoadHistoricalDataAsync(job, default, default);

            var updatedJob = await GetJobAsync(db, job.Id);
            Assert.NotNull(updatedJob);
            Assert.NotNull(updatedJob.ReportId);
            Assert.NotEqual(Guid.Empty, updatedJob.ReportId);

            var report = await GetReportAsync(db, updatedJob.ReportId.Value);
            Assert.NotNull(report);
            Assert.Equal(report.Candlesticks.Count, job.Parameters.Symbols.Count);

            var actrualSymbols = report.Candlesticks
                .Select(x => x.Symbol)
                .ToList();
            var expectedSymbols = candleSticksTestModel
                .Select(x => x.Parameters.Symbol)
                .ToList();

            actrualSymbols.Should().BeEquivalentTo(expectedSymbols);

            foreach (var candlestick in report.Candlesticks)
            {

            }
        }

        private Job CreateJob()
        {
            var jobParameters = new JobParameters(
               ["EURUSD", "USDJPY"],
               DateTime.UtcNow.AddDays(-1),
               DateTime.UtcNow);
            var job = new Job(jobParameters);

            return job;
        }

        private async Task InsertJobToDbAsync(Job job, IMongoDatabase db)
        {
            var jobs = db.GetCollection<Job>(nameof(Job));
            await jobs.InsertOneAsync(job);
        }

        private async Task<Job?> GetJobAsync(
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

        private async Task<Report?> GetReportAsync(
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
            List<Guid>klineIds)
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
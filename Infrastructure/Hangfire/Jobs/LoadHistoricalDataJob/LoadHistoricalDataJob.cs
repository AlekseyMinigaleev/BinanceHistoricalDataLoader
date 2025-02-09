using Domain.Exceptions;
using Domain.Extensions;
using Domain.Models.Job;
using Domain.Models.Kline;
using Domain.Models.Report;
using Infrastructure.Extensions;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob
{
    internal class LoadHistoricalDataJob(
        IMongoDatabase db)
        : ILoadHistoricalDataJob
    {
        private readonly IMongoDatabase _db = db;
        private readonly IMongoCollection<Job> _jobCollection = db.GetCollection<Job>(nameof(Job));

        public async Task LoadHistoricalDataAsync(
            Job job,
            CancellationToken cancellationToken)
        {
            try
            {
                await ProcessAsync(job, cancellationToken);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(job, ex);
            }
        }

        private async Task ProcessAsync(
            Job job,
            CancellationToken cancellationToken)
        {
            var isRetryJob = await IsRetryJobAsync(job.Id, cancellationToken);
            if (isRetryJob)
                throw new JobInterruptedException();

            await SetupJobInitialStateAsync(job, cancellationToken);

            var candlestickWithKlines = await CreateCandlesticksWithLinksAndLinksAsync(
                job.Parameters,
                Interval.OneDay,
                cancellationToken);

            var report = new Report(
                jobId: job.Id,
                candlesticks: candlestickWithKlines.Candlesticks);

            await SaveKlinesToDbAsync(candlestickWithKlines.Kline, cancellationToken);
            await SaveReportToDbAsync(report, cancellationToken);

            await SetJobCompletionResultsAsync(job, report.Id, cancellationToken);
        }

        private async Task<bool> IsRetryJobAsync(Guid jobId, CancellationToken cancellationToken)
        {
            var job = await (await _jobCollection
                .FindAsync(x => x.Id == jobId, cancellationToken: cancellationToken))
                .FirstAsync(cancellationToken);

            return job.Status == JobStatus.InProcessing;
        }

        private async Task SetupJobInitialStateAsync(
            Job job,
            CancellationToken cancellationToken)
        {
            job.Status = JobStatus.InProcessing;
            job.StartTime = DateTime.UtcNow;

            await _jobCollection.ReplaceOneAsync(
                x => x.Id == job.Id,
                job,
                cancellationToken: cancellationToken);
        }

        private async Task<CandlestickWithKlines> CreateCandlesticksWithLinksAndLinksAsync(
            JobParameters parameters,
            Interval interval,
            CancellationToken cancellationToken)
        {
            var candlesticks = new List<Candlestick>();
            var allKlines = new List<Kline>();
            foreach (var symbol in parameters.Symbols)
            {
                var klines = await FetchBinanceData(
                    symbol,
                    parameters.StartDate,
                    parameters.EndDate,
                    interval,
                    cancellationToken);

                allKlines.AddRange(klines);

                var klineIds = klines
                    .Select(x => x.Id)
                    .ToList();

                var candlestick = new Candlestick(
                    symbol: symbol,
                    interval: interval,
                    klineIds: klineIds);

                candlesticks.Add(candlestick);
            }

            return new CandlestickWithKlines(candlesticks, allKlines);
        }

        private async Task<List<Kline>> FetchBinanceData(
            string symbol,
            DateTime startDate,
            DateTime endDate,
            Interval interval,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient();
            var url = $"https://api.binance.com/api/v3/klines" +
                $"?symbol={symbol}" +
                $"&interval={interval.GetDescription()}" +
                $"&startTime={startDate.ToUnixTimeMilliseconds()}" +
                $"&endTime={endDate.ToUnixTimeMilliseconds()}";

            var response = await client.GetAsync(
                url,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content
                .ReadAsStringAsync(cancellationToken);

            return ParseBinanceResponse(responseBody);
        }

        private async Task SaveKlinesToDbAsync(
            List<Kline> klinesToInsert,
            CancellationToken cancellationToken)
        {
            var candlesticks = _db.GetCollection<Kline>(nameof(Kline));
            await candlesticks.InsertManyAsync(
                klinesToInsert,
                cancellationToken: cancellationToken);
        }

        private async Task SaveReportToDbAsync(
            Report report,
            CancellationToken cancellationToken)
        {
            var reports = _db.GetCollection<Report>(nameof(Report));
            await reports.InsertOneAsync(
                report,
                cancellationToken: cancellationToken);
        }

        private List<Kline> ParseBinanceResponse(string responseBody)
        {
            var klines = new List<Kline>();
            var rows = JArray.Parse(responseBody);

            foreach (var row in rows)
                klines.Add(new Kline(
                    openTime: (long)row[0],
                    closeTime: (long)row[6],
                    openPrice: Convert.ToDouble(row[1]),
                    highPrice: Convert.ToDouble(row[2]),
                    lowPrice: Convert.ToDouble(row[3]),
                    closePrice: Convert.ToDouble(row[4]),
                    volume: Convert.ToDouble(row[5]),
                    quoteAssetVolume: Convert.ToDouble(row[7]),
                    numberOfTrades: Convert.ToInt32(row[8]),
                    takerBuyBaseAssetVolume: Convert.ToDouble(row[9]),
                    takerBuyQuoteAssetVolume: Convert.ToDouble(row[10])));

            return klines;
        }

        private async Task SetJobCompletionResultsAsync(
            Job job,
            Guid reportId,
            CancellationToken cancellationToken)
        {
            job.Status = JobStatus.Completed;
            job.EndTime = DateTime.UtcNow;
            job.ReportId = reportId;

            await _jobCollection.ReplaceOneAsync(
                x => x.Id == job.Id,
                job,
                cancellationToken: cancellationToken);
        }

        private async Task HandleExceptionAsync(Job job, Exception ex)
        {
            job.Status = JobStatus.Error;
            job.ErrorMessage = ex.Message;

            await _jobCollection.ReplaceOneAsync(
                x => x.Id == job.Id,
                job);
        }
    }
}
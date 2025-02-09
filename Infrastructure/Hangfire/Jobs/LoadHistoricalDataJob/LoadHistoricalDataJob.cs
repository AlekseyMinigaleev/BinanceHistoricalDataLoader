using Domain.Extensions;
using Domain.Models.Candlestick;
using Domain.Models.Job;
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
            await SetupJobInitialStateAsync(job, cancellationToken);

            var candlestickWithLinks = await CreateCandlesticksWithLinksAndLinksAsync(
                job.Parameters,
                Interval.OneDay,
                cancellationToken);

            var report = new Report(
                jobId: job.Id,
                candlestickLinks: candlestickWithLinks.CandlestickLinks);

            await SaveCandleSticksToDbAsync(candlestickWithLinks.Candlesticks, cancellationToken);
            await SaveReportToDbAsync(report, cancellationToken);

            await SetJobCompletionResultsAsync(job, report.Id, cancellationToken);
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

        private async Task<CandlestickWithLinksDTO> CreateCandlesticksWithLinksAndLinksAsync(
            JobParameters parameters,
            Interval interval,
            CancellationToken cancellationToken)
        {
            var candlesticks = new List<Candlestick>();
            var candlestickLinks = new List<CandlestickLink>();
            foreach (var symbol in parameters.Symbols)
            {
                var klines = await FetchBinanceData(
                symbol,
                    parameters.StartDate,
                    parameters.EndDate,
                    interval,
                    cancellationToken);

                var candlestick = new Candlestick(
                    symbol: symbol,
                    interval: interval,
                    data: klines);

                candlesticks.Add(candlestick);

                candlestickLinks.Add(new CandlestickLink(
                    symbol: symbol,
                    candlestickId: candlestick.Id));
            }

            return new CandlestickWithLinksDTO(candlesticks, candlestickLinks);
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
                $"&interval={interval.ToApiString()}" +
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

        private async Task SaveCandleSticksToDbAsync(
            List<Candlestick> candlesticksToInsert,
            CancellationToken cancellationToken)
        {
            var candlesticks = _db.GetCollection<Candlestick>(nameof(Candlestick));
            await candlesticks.InsertManyAsync(
                candlesticksToInsert,
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
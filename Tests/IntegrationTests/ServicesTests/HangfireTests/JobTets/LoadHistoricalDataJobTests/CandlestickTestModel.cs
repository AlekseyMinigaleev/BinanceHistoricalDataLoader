using Domain.Models.Kline;
using Tests.IntegrationTests.HttpUtils.Klines.Klines;

namespace Tests.IntegrationTests.ServicesTests.HangfireTests.JobTets.LoadHistoricalDataJobTests
{
    internal class CandlestickTestModel(Parameters parameters, List<Kline> klines)
    {
        public Parameters Parameters { get; set; } = parameters;
        public List<Kline> Klines { get; set; } = klines;
    }
}
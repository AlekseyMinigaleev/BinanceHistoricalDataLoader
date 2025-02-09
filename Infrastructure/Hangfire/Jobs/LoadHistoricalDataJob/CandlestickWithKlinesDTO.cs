using Domain.Models.Kline;
using Domain.Models.Report;

namespace Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob
{
    internal class CandlestickWithKlinesDTO(
        List<Candlestick> candlesticks,
        List<Kline> klines)
    {
        public List<Candlestick> Candlesticks { get; set; } = candlesticks;
        public List<Kline> Kline { get; set; } = klines;
    }
}

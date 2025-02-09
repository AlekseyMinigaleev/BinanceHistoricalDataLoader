using Domain.Models.Candlestick;
using Domain.Models.Report;

namespace Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob
{
    internal class CandlestickWithLinksDTO(
        List<Candlestick> candlesticks,
        List<CandlestickLink> candlestickLinks)
    {
        public List<Candlestick> Candlesticks { get; set; } = candlesticks;
        public List<CandlestickLink> CandlestickLinks { get; set; } = candlestickLinks;
    }
}

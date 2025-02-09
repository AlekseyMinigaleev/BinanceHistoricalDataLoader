namespace Domain.Models.Report
{
    public class CandlestickLink(
        string symbol,
        Guid candlestickId)
    {
        public string Symbol { get; set; } = symbol;

        public Guid CandlestickId { get; set; } = candlestickId;
    }
}
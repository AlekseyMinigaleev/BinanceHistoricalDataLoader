namespace Domain.Models.Report
{
    public class Candlestick(
        string symbol,
        Interval interval,
        ICollection<Guid> klineIds)
    {
        public string Symbol { get; set; } = symbol;

        public Interval Interval { get; set; } = interval;

        public ICollection<Guid> KlineIds { get; set; } = klineIds;
    }
}
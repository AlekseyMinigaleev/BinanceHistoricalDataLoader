namespace Domain.Models.Candlestick
{
    public class Candlestick(
        string symbol,
        Interval interval,
        ICollection<Kline> data) : BaseModel()
    {
        public string Symbol { get; set; } = symbol;

        public Interval Interval { get; set; } = interval;

        public ICollection<Kline> Data { get; set; } = data;
    }
}
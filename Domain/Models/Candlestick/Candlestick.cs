namespace Domain.Models.Candlestick
{
    public class Candlestick : BaseModel
    {
        public string Symbol { get; set; }

        public Interval Interval { get; set; }

        public ICollection<Kline> Data { get; set; }
    }
}
namespace Domain.Models.Candlestick
{
    public class Kline
    {
        public long OpenTime { get; set; }

        public long CloseTime { get; set; }

        public double OpenPrice { get; set; }

        public double HighPrice { get; set; }

        public double LowPrice { get; set; }

        public double ClosePrice { get; set; }

        public double Volume { get; set; }

        public double QuoteAssetVolume { get; set; }

        public int NumberOfTrades { get; set; }

        public double TakerBuyBaseAssetVolume { get; set; }

        public double TakerBuyQuoteAssetVolume { get; set; }
    }
}
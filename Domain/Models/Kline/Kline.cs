namespace Domain.Models.Kline
{
    public class Kline(
        long openTime,
        long closeTime,
        double openPrice,
        double highPrice,
        double lowPrice,
        double closePrice,
        double volume,
        double quoteAssetVolume,
        int numberOfTrades,
        double takerBuyBaseAssetVolume,
        double takerBuyQuoteAssetVolume)
        : BaseModel()
    {
        public long OpenTime { get; set; } = openTime;

        public long CloseTime { get; set; } = closeTime;

        public double OpenPrice { get; set; } = openPrice;

        public double HighPrice { get; set; } = highPrice;

        public double LowPrice { get; set; } = lowPrice;

        public double ClosePrice { get; set; } = closePrice;

        public double Volume { get; set; } = volume;

        public double QuoteAssetVolume { get; set; } = quoteAssetVolume;

        public int NumberOfTrades { get; set; } = numberOfTrades;

        public double TakerBuyBaseAssetVolume { get; set; } = takerBuyBaseAssetVolume;

        public double TakerBuyQuoteAssetVolume { get; set; } = takerBuyQuoteAssetVolume;
    }
}
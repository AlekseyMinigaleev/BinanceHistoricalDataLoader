using Domain.Extensions;
using Domain.Models.Kline;
using Newtonsoft.Json.Linq;
using Tests.IntegrationTests.HttpUtils.Klines.Klines;

namespace Tests.IntegrationTests.HttpUtils.Klines
{
    internal class KlinesHttpUtils(IHttpClientFactory httpClientFactory)
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public readonly string URL = "https://api.binance.com/api/v3/klines";

        public async Task<List<Kline>> FetchDataAsync(
            Parameters parameters,
            CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();

            var url = URL +
                $"?symbol={parameters.Symbol}" +
                $"&interval={parameters.Interval.GetDescription()}" +
                $"&startTime={new DateTimeOffset(parameters.StartDate).ToUnixTimeMilliseconds()}" +
                $"&endTime={new DateTimeOffset(parameters.EndDate).ToUnixTimeMilliseconds()}";

            var response = await client.GetAsync(
                url,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content
                .ReadAsStringAsync(cancellationToken);

            return ParseBinanceResponse(responseBody);
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
    }
}

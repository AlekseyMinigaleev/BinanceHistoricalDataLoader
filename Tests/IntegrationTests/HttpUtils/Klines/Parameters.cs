using Domain.Models.Report;

namespace Tests.IntegrationTests.HttpUtils.Klines.Klines
{
    internal class Parameters(
        string symbol,
        DateTime startDate,
        DateTime endDate,
        Interval interval)
    {
        public string Symbol { get; set; } = symbol;

        public DateTime StartDate { get; set; } = startDate;

        public DateTime EndDate { get; set; } = endDate;

        public Interval Interval { get; set; } = interval;
    }
}

namespace Domain.Models.Report
{
    public class Report(
        Guid jobId,
        ICollection<CandlestickLink> candlestickLinks)
        : BaseModel()
    {
        public Guid JobId { get; set; } = jobId;

        public ICollection<CandlestickLink> CandlestickLinks { get; set; } = candlestickLinks;
    }
}
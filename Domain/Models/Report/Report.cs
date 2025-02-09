namespace Domain.Models.Report
{
    public class Report(
        Guid jobId,
        ICollection<Candlestick> candlesticks)
        : BaseModel()
    {
        public Guid JobId { get; set; } = jobId;

        public ICollection<Candlestick> Candlesticks { get; set; } = candlesticks;
    }
}
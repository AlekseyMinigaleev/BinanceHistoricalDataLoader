namespace Domain.Models.Report
{
    public class Report : BaseModel
    {
        public Guid JobId { get; set; }

        public ICollection<CandlestickLink> CandlestickLinks { get; set; }
    }
}
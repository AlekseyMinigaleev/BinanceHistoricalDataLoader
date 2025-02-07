namespace Domain.Models.Job
{
    public class Job : BaseModel
    {
        public JobStatus Status { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public Guid? ReportId { get; set; }

        public JopParameters Parameters { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
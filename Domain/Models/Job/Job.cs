namespace Domain.Models.Job
{
    public class Job : BaseModel
    {
        public JobParameters Parameters { get; set; }

        public JobStatus Status { get; set; } = JobStatus.NotStarted;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public Guid? ReportId { get; set; }

        public string? ErrorMessage { get; set; }

        public Job(JobParameters parameters) : base()
        {
            Parameters = parameters;
        }

        private Job() :base()
        { }
    }
}
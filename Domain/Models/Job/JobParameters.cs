namespace Domain.Models.Job
{
    public class JobParameters
    {
        public ICollection<string> Symbols { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public JobParameters(
            ICollection<string> symbols,
            DateTime startDate,
            DateTime endDate)
        {
            Symbols = symbols;
            StartDate = startDate;
            EndDate = endDate;
        }

        private JobParameters()
        { }
    }
}
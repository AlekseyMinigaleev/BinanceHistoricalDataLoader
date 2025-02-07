namespace Domain.Models.Job
{
    public class JopParameters
    {
        public ICollection<string> Symbols { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
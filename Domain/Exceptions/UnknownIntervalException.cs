namespace Domain.Exceptions
{
    public class UnknownIntervalException : Exception
    {
        public string? Interval { get; }

        public UnknownIntervalException(
            string interval) : base($"Unknown interval: {interval}")
        {
            Interval = interval;
        }

        public UnknownIntervalException() : base($"Unknown interval")
        { }
    }
}
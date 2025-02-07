namespace Domain.Exceptions
{
    public class MissingApiStringAttributeException(
        string interval)
        : Exception($"The interval value '{interval}' does not have the required [EnumMember] attribute.")
    {
        public string Interval { get; } = interval;
    }
}

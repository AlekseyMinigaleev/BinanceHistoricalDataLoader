namespace Domain.Exceptions
{
    public class ConfigurationException(string name)
        : InvalidOperationException($"{name} is not configured properly")
    { }
}
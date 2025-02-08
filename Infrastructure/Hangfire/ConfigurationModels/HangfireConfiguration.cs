namespace Infrastructure.Hangfire.ConfigurationModels
{
    internal class HangfireConfiguration
    {
        public int CompatibilityLevel { get; set; }
        public string DatabaseName { get; set; }
        public string Prefix { get; set; }
    }
}
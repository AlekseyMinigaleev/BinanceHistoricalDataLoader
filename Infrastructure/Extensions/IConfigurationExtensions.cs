using Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions
{
    internal static class IConfigurationExtensions
    {
        public static T GetConfigurationModel<T>(
           this IConfiguration configuration,
           string sectionPath)
        {
            var configurationModel = configuration
                .GetSection(sectionPath)
                .Get<T>()
                ?? throw new ConfigurationException(sectionPath);

            return configurationModel;
        }
    }
}
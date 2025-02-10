using Domain.Models.Job;
using Hangfire;
using Hangfire.Server;

namespace Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob
{
    public interface ILoadHistoricalDataJob
    {
        [AutomaticRetry(Attempts = 0)]
        public Task LoadHistoricalDataAsync(Job job, PerformContext performContext, CancellationToken cancellationToken);
    }
}
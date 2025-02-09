using Domain.Models.Job;
using Hangfire.Server;

namespace Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob
{
    public interface ILoadHistoricalDataJob
    {
        public Task LoadHistoricalDataAsync(Job job, PerformContext performContext, CancellationToken cancellationToken);
    }
}
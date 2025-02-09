using Domain.Models.Job;

namespace Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob
{
    public interface ILoadHistoricalDataJob
    {
        public Task LoadHistoricalDataAsync(Job job, CancellationToken cancellationToken);
    }
}

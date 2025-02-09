using AutoMapper;
using Domain.Extensions;
using Domain.Models.Job;
using MediatR;
using MongoDB.Driver;

namespace API.Controllers.HistoricalData.Actions
{
    public class Status
    {
        public class StatusQuery : IRequest<StatusResponse?>
        {
            public Guid JobId { get; set; }
        }

        public class StatusResponse
        {
            public Guid JobId { get; set; }
            public string Status { get; set; }
            public DateTime? EndTime { get; set; }
            public Guid? ReportId { get; set; }
        }

        public class Profiler : Profile
        {
            public Profiler()
            {
                CreateMap<Job, StatusResponse>()
                    .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.GetDescription()));
            }
        }

        public class Handler(
            IMapper mapper,
            IMongoDatabase db)
            : IRequestHandler<StatusQuery, StatusResponse>
        {
            private readonly IMongoDatabase _db = db;
            private readonly IMongoCollection<Job> _jobCollection = db.GetCollection<Job>(nameof(Job));
            private readonly IMapper _mapper = mapper;

            public async Task<StatusResponse?> Handle(
                StatusQuery request,
                CancellationToken cancellationToken)
            {
                var job = await (await _jobCollection
                    .FindAsync(x => x.Id == request.JobId, cancellationToken: cancellationToken))
                    .SingleOrDefaultAsync(cancellationToken);

                if(job is null)
                    return null;

                var response = _mapper.Map<StatusResponse>(job);

                return response;
            }
        }
    }
}
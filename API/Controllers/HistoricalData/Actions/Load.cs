﻿using AutoMapper;
using Domain.Models.Job;
using Hangfire;
using Infrastructure.Hangfire.Jobs.LoadHistoricalDataJob;
using MediatR;
using MongoDB.Driver;

namespace API.Controllers.HistoricalData.Actions
{
    public class Load
    {
        public class LoadQuery : IRequest<Guid>
        {
            public ICollection<string> Pairs { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }
        }

        public class Profiler : Profile
        {
            public Profiler()
            {
                CreateMap<LoadQuery, Job>()
                    .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src => src))
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.Status, opt => opt.Ignore())
                    .ForMember(dest => dest.StartTime, opt => opt.Ignore())
                    .ForMember(dest => dest.EndTime, opt => opt.Ignore())
                    .ForMember(dest => dest.ReportId, opt => opt.Ignore())
                    .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore());

                CreateMap<LoadQuery, JobParameters>()
                    .ForMember(dest => dest.Symbols, opt => opt.MapFrom(src => src.Pairs));
            }
        }

        public class Handler(
            IMapper mapper,
            IMongoDatabase db,
            IBackgroundJobClient backgroundJobClient)
            : IRequestHandler<LoadQuery, Guid>
        {
            private readonly IMapper _mapper = mapper;
            private readonly IMongoDatabase _db = db;
            private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

            public async Task<Guid> Handle(
                LoadQuery request,
                CancellationToken cancellationToken)
            {
                var job = _mapper.Map<Job>(request);

                var jobs = _db.GetCollection<Job>(nameof(Job));
                await jobs.InsertOneAsync(job, cancellationToken: cancellationToken);

                _backgroundJobClient.Enqueue<ILoadHistoricalDataJob>(service =>
                    service.LoadHistoricalDataAsync(job, cancellationToken));

                return job.Id;
            }
        }
    }
}
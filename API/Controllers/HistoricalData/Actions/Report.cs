using AutoMapper;
using Domain.Extensions;
using Domain.Models.Candlestick;
using MediatR;
using MongoDB.Driver;

namespace API.Controllers.HistoricalData.Actions
{
    public class Report
    {
        public class ReportQuery : IRequest<CandlestickVM[]>
        {
            public Guid Id { get; set; }
        }

        public class CandlestickVM
        {
            public string Symbol { get; set; }
            public string Interval { get; set; }
            public ICollection<KlineVM> Data { get; set; }
        }

        public class KlineVM
        {
            public long OpenTime { get; set; }
            public long CloseTime { get; set; }
            public double OpenPrice { get; set; }
            public double HighPrice { get; set; }
            public double LowPrice { get; set; }
            public double ClosePrice { get; set; }
            public double Volume { get; set; }
            public double QuoteAssetVolume { get; set; }
            public int NumberOfTrades { get; set; }
            public double TakerBuyBaseAssetVolume { get; set; }
            public double TakerBuyQuoteAssetVolume { get; set; }
        }

        public class Profiler : Profile
        {
            public Profiler()
            {
                CreateMap<Candlestick, CandlestickVM>()
                    .ForMember(dest => dest.Interval, opt => opt.MapFrom(src => src.Interval.GetDescription()))
                    .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

                CreateMap<Kline, KlineVM>();
            }
        }

        public class Handler(
            IMongoDatabase database,
            IMapper mapper)
            : IRequestHandler<ReportQuery, CandlestickVM[]>
        {
            private readonly IMongoDatabase _database = database;
            private readonly IMapper _mapper = mapper;

            public async Task<CandlestickVM[]> Handle(
                ReportQuery request,
                CancellationToken cancellationToken)
            {
                var report = await GetReportByIdAsync(request.Id, cancellationToken);
                if (report is null)
                    return null;

                var candlestickIds = report.CandlestickLinks
                    .Select(x => x.CandlestickId);

                var candlesticks = await GetCandleSticksByIdsAsync(candlestickIds, cancellationToken);

                var candlestickVMs = _mapper.Map<CandlestickVM[]>(candlesticks);

                return candlestickVMs;
            }

            private async Task<Domain.Models.Report.Report?> GetReportByIdAsync(
                Guid id,
                CancellationToken cancellationToken)
            {
                var reports = _database.GetCollection<Domain.Models.Report.Report>(
                    nameof(Domain.Models.Report.Report));

                var report = await reports
                    .Find(x => x.Id == id)
                    .SingleOrDefaultAsync(cancellationToken);

                return report;
            }

            private async Task<List<Candlestick>> GetCandleSticksByIdsAsync(
                IEnumerable<Guid> candlestickIds,
                CancellationToken cancellationToken)
            {
                var candlesticks = _database.GetCollection<Candlestick>(nameof(Candlestick));

                var filter = Builders<Candlestick>.Filter
                    .In(x => x.Id, candlestickIds);

                var resultCandlesticks = await candlesticks
                    .Find(filter)
                    .ToListAsync(cancellationToken);

                return resultCandlesticks;
            }
        }
    }
}
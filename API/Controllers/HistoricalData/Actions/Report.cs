using AutoMapper;
using Domain.Extensions;
using Domain.Models.Kline;
using Domain.Models.Report;
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
                    .ForMember(dest => dest.Data, opt => opt.Ignore());

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

                var klineIdsByCandlestickDictionary = report.Candlesticks
                    .ToDictionary(key => key, value => value.KlineIds);

                var result = new List<CandlestickVM>();

                foreach (var kvp in klineIdsByCandlestickDictionary)
                {
                    var candlestickVM = _mapper.Map<CandlestickVM>(kvp.Key);
                    var klines = await GetKlinesByIdsAsync(kvp.Value, cancellationToken);
                    var klineVms = _mapper.Map<List<KlineVM>>(klines);
                    candlestickVM.Data = klineVms;

                    result.Add(candlestickVM);
                }

                return [.. result];
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

            private async Task<List<Kline>> GetKlinesByIdsAsync(
                IEnumerable<Guid> klineIds,
                CancellationToken cancellationToken)
            {
                var candlesticks = _database.GetCollection<Kline>(nameof(Kline));

                var filter = Builders<Kline>.Filter
                    .In(x => x.Id, klineIds);

                var resultCandlesticks = await candlesticks
                    .Find(filter)
                    .ToListAsync(cancellationToken);

                return resultCandlesticks;
            }
        }
    }
}
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Domain.Alerts.DTOs;
using SiteWatcher.Application.Alerts.ViewModels;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace SiteWatcher.Benchmark.Benchs;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class AutoMapperVsManualMapping
{
    public AutoMapperVsManualMapping()
    {
        _idHasher = new IdHasher(new TestAppSettings());
        var alertsDtos = new List<SimpleAlertViewDto>();
        foreach (var index in Enumerable.Range(0, 10))
        {
            alertsDtos.Add(new SimpleAlertViewDto
            {
                Id = index,
                Frequency = Frequencies.EightHours,
                Name = $"Alert {index}",
                CreatedAt = DateTime.Now,
                LastVerification = DateTime.Now.AddDays(-index),
                NotificationsSent = 12,
                SiteName = $"Site name {index}",
                WatchMode = index % 2 == 0 ? 'A' : 'T'
            });
        }

        _paginatedAlertsDto = new PaginatedList<SimpleAlertViewDto>(alertsDtos, 50);
    }

    private readonly IIdHasher _idHasher;
    private IMapper _mapper;
    private readonly PaginatedList<SimpleAlertViewDto> _paginatedAlertsDto;

    [GlobalSetup]
    public void SetupAutoMapper()
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(PaginatedList<>), typeof(PaginatedList<>));
            cfg.CreateMap<SimpleAlertViewDto, SimpleAlertView>()
                .ForMember(opt => opt.WatchMode, opt =>
                    opt.MapFrom(src => Utils.GetWatchModeEnumByTableDiscriminator(src.WatchMode)))
                .AfterMap<HashIdMapping>();
        });

        _mapper = mapperConfiguration.CreateMapper();
    }

    [Benchmark]
    public PaginatedList<SimpleAlertView> AutoMapping()
    {
        return _mapper.Map<PaginatedList<SimpleAlertView>>(_paginatedAlertsDto);
    }

    [Benchmark]
    public PaginatedList<SimpleAlertView> ManualMapping()
    {
        return new PaginatedList<SimpleAlertView>
        {
            Total = _paginatedAlertsDto.Total,
            Results = _paginatedAlertsDto.Results.Select(dto => SimpleAlertView.FromDto(dto, _idHasher))
        };
    }
}

public class HashIdMapping : IMappingAction<SimpleAlertViewDto, SimpleAlertView>
{
    private readonly IIdHasher _idHasher = new IdHasher(new TestAppSettings());

    public void Process(SimpleAlertViewDto source, SimpleAlertView destination, ResolutionContext context)
    {
        destination.Id = _idHasher.HashId(source.Id);
    }
}
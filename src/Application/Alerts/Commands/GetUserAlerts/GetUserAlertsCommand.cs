using AutoMapper;
using Domain.DTOs.Common;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.Commands.GetUserAlerts;

public class GetUserAlertsCommand : IRequest<ICommandResult<PaginatedList<SimpleAlertView>>>, ICacheable
{
    public string? LastAlertId { get; set; }
    public int Take { get; set; } = 10;

    public string GetKey(ISession session) =>
        CacheKeys.GetAlerts(session.UserId!.Value);

    public string HashFieldName =>
        $"{Take}-{LastAlertId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetUserAlertsCommandHandler :
    IRequestHandler<GetUserAlertsCommand, ICommandResult<PaginatedList<SimpleAlertView>>>
{
    private readonly IIdHasher _idHasher;
    private readonly ISession _session;
    private readonly IAlertDapperRepository _alertDapperRepository;
    private readonly IMapper _mapper;

    public GetUserAlertsCommandHandler(IIdHasher idHasher, ISession session, IAlertDapperRepository alertDapperRepository,
        IMapper mapper)
    {
        _idHasher = idHasher;
        _session = session;
        _alertDapperRepository = alertDapperRepository;
        _mapper = mapper;
    }

    public async Task<ICommandResult<PaginatedList<SimpleAlertView>>> Handle(GetUserAlertsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Take == 0)
            return new CommandResult<PaginatedList<SimpleAlertView>>(new PaginatedList<SimpleAlertView>(Array.Empty<SimpleAlertView>()));

        var take = request.Take > 50 ? 50 : request.Take;
        var lastAlertId = string.IsNullOrEmpty(request.LastAlertId) ? 0 : _idHasher.DecodeId(request.LastAlertId);

        var alertsDto = await _alertDapperRepository
            .GetUserAlerts(_session.UserId!.Value, take, lastAlertId, cancellationToken);

        var alertsView = _mapper.Map<PaginatedList<SimpleAlertView>>(alertsDto);
        return new CommandResult<PaginatedList<SimpleAlertView>>(alertsView);
    }
}
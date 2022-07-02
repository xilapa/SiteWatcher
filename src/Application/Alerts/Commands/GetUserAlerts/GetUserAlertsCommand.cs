using AutoMapper;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Alerts.Commands.GetUserAlerts;

public class GetUserAlertsCommand : IRequest<ICommandResult<IEnumerable<SimpleAlertView>>>
{
    public string LastAlertId { get; set; }
    public int Take { get; set; } = 10;
}

public class GetUserAlertsCommandHandler :
    IRequestHandler<GetUserAlertsCommand, ICommandResult<IEnumerable<SimpleAlertView>>>
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

    public async Task<ICommandResult<IEnumerable<SimpleAlertView>>> Handle(GetUserAlertsCommand request,
        CancellationToken cancellationToken)
    {
        var lastAlertId = string.IsNullOrEmpty(request.LastAlertId) ? 0 : _idHasher.DecodeId(request.LastAlertId);
        var alertsDto = await _alertDapperRepository
            .GetUserAlerts(_session.UserId!.Value, request.Take, lastAlertId, cancellationToken);

        var alertsView = _mapper.Map<IEnumerable<SimpleAlertView>>(alertsDto);
        return new CommandResult<IEnumerable<SimpleAlertView>>(alertsView);
    }
}
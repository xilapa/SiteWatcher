using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.Commands.GetUserAlerts;

public class GetUserAlertsCommand : IRequest<CommandResult>, ICacheable
{
    public string? LastAlertId { get; set; }
    public int Take { get; set; } = 10;

    public string GetKey(ISession session) =>
        CacheKeys.UserAlerts(session.UserId!.Value);

    public string HashFieldName =>
        $"Take:{Take}-LastId:{LastAlertId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetUserAlertsCommandHandler : IRequestHandler<GetUserAlertsCommand, CommandResult>
{
    private readonly IIdHasher _idHasher;
    private readonly ISession _session;
    private readonly IAlertDapperRepository _alertDapperRepository;

    public GetUserAlertsCommandHandler(IIdHasher idHasher, ISession session, IAlertDapperRepository alertDapperRepository)
    {
        _idHasher = idHasher;
        _session = session;
        _alertDapperRepository = alertDapperRepository;
    }

    public async Task<CommandResult> Handle(GetUserAlertsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Take == 0)
            return CommandResult.Empty();

        var take = request.Take > 50 ? 50 : request.Take;
        var lastAlertId = string.IsNullOrEmpty(request.LastAlertId) ? 0 : _idHasher.DecodeId(request.LastAlertId);

        var paginatedListAlertsDto = await _alertDapperRepository
            .GetUserAlerts(_session.UserId!.Value, take, lastAlertId, cancellationToken);

        return CommandResult.FromValue(paginatedListAlertsDto);
    }
}
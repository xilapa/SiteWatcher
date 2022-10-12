using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.Commands.GetAlertDetails;

public class GetAlertDetailsCommand : IRequest<CommandResult>, ICacheable
{
    public string AlertId { get; set; }

    public TimeSpan Expiration =>
        TimeSpan.FromMinutes(10);

    public string HashFieldName =>
        $"AlertId:{AlertId}";

    public string GetKey(ISession session) =>
        CacheKeys.UserAlerts(session.UserId!.Value);
}

public class GetAlertDetailsCommandHandler : IRequestHandler<GetAlertDetailsCommand, CommandResult>
{
    private readonly ISession _session;
    private readonly IAlertDapperRepository _alertDapperRepository;
    private readonly IIdHasher _idHasher;

    public GetAlertDetailsCommandHandler(ISession session, IAlertDapperRepository alertDapperRepository,
        IIdHasher idHasher)
    {
        _session = session;
        _alertDapperRepository = alertDapperRepository;
        _idHasher = idHasher;
    }

    public async Task<CommandResult> Handle(GetAlertDetailsCommand request,
        CancellationToken cancellationToken)
    {
        var alertId = _idHasher.DecodeId(request.AlertId);
        if (alertId == 0)
            return CommandResult.Empty();

        var alertDetailsDto =
            await _alertDapperRepository.GetAlertDetails(alertId, _session.UserId!.Value,
                cancellationToken);

        return alertDetailsDto is null ?
            CommandResult.Empty() :
            CommandResult.FromValue(AlertDetails.FromDto(alertDetailsDto, _idHasher));
    }
}
using MediatR;
using SiteWatcher.Application.Alerts.ViewModels;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.Commands.GetAlertDetails;

public class GetAlertDetailsCommand : IRequest<AlertDetails?>, ICacheable
{
    public string AlertId { get; set; }

    public TimeSpan Expiration =>
        TimeSpan.FromMinutes(60);

    public string HashFieldName =>
        $"AlertId:{AlertId}";

    public string GetKey(ISession session) =>
        CacheKeys.UserAlerts(session.UserId!.Value);
}

public class GetAlertDetailsCommandHandler : IRequestHandler<GetAlertDetailsCommand, AlertDetails?>
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

    public async Task<AlertDetails?> Handle(GetAlertDetailsCommand request, CancellationToken cancellationToken)
    {
        var alertId = _idHasher.DecodeId(request.AlertId);
        if (alertId == 0)
            return null;

        var alertDetails = await _alertDapperRepository.GetAlertDetails(alertId, _session.UserId!.Value,
                cancellationToken);
        return alertDetails;
    }
}
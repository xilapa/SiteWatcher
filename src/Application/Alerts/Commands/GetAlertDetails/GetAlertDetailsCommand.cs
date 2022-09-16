using AutoMapper;
using MediatR;
using SiteWatcher.Application.Common.Commands;
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

public class GetAlerDetailsCommandHandler : IRequestHandler<GetAlertDetailsCommand, CommandResult>
{
    private readonly ISession _session;
    private readonly IAlertDapperRepository _alertDapperRepository;
    private readonly IIdHasher _idHasher;
    private readonly IMapper _mapper;

    public GetAlerDetailsCommandHandler(ISession session, IAlertDapperRepository alertDapperRepository,
        IIdHasher idHasher, IMapper mapper)
    {
        _session = session;
        _alertDapperRepository = alertDapperRepository;
        _idHasher = idHasher;
        _mapper = mapper;
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

        if (alertDetailsDto is null)
            return CommandResult.Empty();

        var alertDetail = _mapper.Map<AlertDetails>(alertDetailsDto);
        return CommandResult.FromValue(alertDetail);
    }
}
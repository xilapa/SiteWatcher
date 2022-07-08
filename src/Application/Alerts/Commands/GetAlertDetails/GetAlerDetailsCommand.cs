using AutoMapper;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.Commands.GetAlertDetails;

public class GetAlerDetailsCommand : IRequest<ICommandResult<AlertDetails>>, ICacheable
{
    public string AlertId { get; set; }

    public TimeSpan Expiration =>
        TimeSpan.FromMinutes(10);

    public string HashFieldName =>
        $"AlertId:{AlertId}";

    public string GetKey(ISession session) =>
        CacheKeys.UserAlerts(session.UserId!.Value);
}

public class GetAlerDetailsCommandHandler : IRequestHandler<GetAlerDetailsCommand, ICommandResult<AlertDetails>>
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

    public async Task<ICommandResult<AlertDetails>> Handle(GetAlerDetailsCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AlertId))
            return new CommandResult<AlertDetails>();

        var alertId = _idHasher.DecodeId(request.AlertId);
        if (alertId == 0)
            return new CommandResult<AlertDetails>();

        var alertDetailsDto =
            await _alertDapperRepository.GetAlertDetails(alertId, _session.UserId!.Value,
                cancellationToken);

        if (alertDetailsDto is null)
            return new CommandResult<AlertDetails>();

        var alertDetail = _mapper.Map<AlertDetails>(alertDetailsDto);
        return new CommandResult<AlertDetails>(alertDetail);
    }
}
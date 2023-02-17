﻿using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Repositories;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;

namespace SiteWatcher.Application.Alerts.Commands.GetUserAlerts;

public class GetUserAlertsCommand : IRequest<CommandResult>, ICacheable
{
    public string? LastAlertId { get; set; }
    public int Take { get; set; } = 10;

    public string GetKey(ISession session) =>
        CacheKeys.UserAlerts(session.UserId);

    public string HashFieldName =>
        $"Take:{Take}-LastId:{LastAlertId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(60);
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

    public async Task<CommandResult> Handle(GetUserAlertsCommand request, CancellationToken cancellationToken)
    {
        if (request.Take == 0)
            return CommandResult.Empty();

        var take = request.Take > 50 ? 50 : request.Take;
        var lastAlertId = string.IsNullOrEmpty(request.LastAlertId) ? 0 : _idHasher.DecodeId(request.LastAlertId);

        var paginatedListAlerts = await _alertDapperRepository
            .GetUserAlerts(_session.UserId, take, lastAlertId, cancellationToken);

        return CommandResult.FromValue(paginatedListAlerts);
    }
}
using Application.Alerts.Dtos;
using Dapper;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Queries;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Alerts.Commands.GetAlertDetails;

public class GetAlertDetailsQuery : ICacheable
{
    public string? AlertId { get; set; }

    public TimeSpan Expiration =>
        TimeSpan.FromMinutes(60);

    public string HashFieldName =>
        $"AlertId:{AlertId}";

    public string GetKey(ISession session) =>
        CacheKeys.UserAlerts(session.UserId!.Value);
}

public class GetAlertDetailsQueryHandler : IApplicationHandler
{
    private readonly ISession _session;
    private readonly IDapperContext _context;
    private readonly IQueries _queries;
    private readonly IIdHasher _idHasher;

    public GetAlertDetailsQueryHandler(ISession session, IDapperContext context, IQueries queries,  IIdHasher idHasher)
    {
        _session = session;
        _context = context;
        _queries = queries;
        _idHasher = idHasher;
    }

    public async ValueTask<AlertDetails?> Handle(GetAlertDetailsQuery request, CancellationToken cancellationToken)
    {
        if(request.AlertId == null)
            return default;

        var alertId = _idHasher.DecodeId(request.AlertId);
        if (alertId == 0)
            return null;

        var query = _queries.GetAlertDetails(_session.UserId!.Value, new AlertId(alertId));

        var alertDetailsDto = await _context
            .UsingConnectionAsync(conn =>
            {
                var command = new CommandDefinition(
                    query.Sql,
                    query.Parameters,
                    cancellationToken: cancellationToken);
                return conn.QueryFirstOrDefaultAsync<AlertDetailsDto?>(command);
            });
        return alertDetailsDto?.ToAlertDetails(_idHasher);
    }
}
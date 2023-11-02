using Application.Alerts.Dtos;
using Dapper;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Queries;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Alerts.Commands.GetUserAlerts;

public class GetUserAlertsQuery : ICacheable
{
    public string? LastAlertId { get; set; }
    public int Take { get; set; } = 10;

    public string GetKey(ISession session) =>
        CacheKeys.UserAlerts(session.UserId!.Value);

    public string HashFieldName =>
        $"Take:{Take}-LastId:{LastAlertId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(60);
}

public class GetUserAlertsQueryHandler : IApplicationHandler
{
    private readonly IIdHasher _idHasher;
    private readonly ISession _session;
    private readonly IDapperContext _context;
    private readonly IQueries _queries;

    public GetUserAlertsQueryHandler(IIdHasher idHasher, ISession session, IDapperContext context, IQueries queries)
    {
        _idHasher = idHasher;
        _session = session;
        _context = context;
        _queries = queries;
    }

    public async Task<Result<PaginatedList<SimpleAlertView>>> Handle(GetUserAlertsQuery request, CancellationToken cancellationToken)
    {
        if (request.Take == 0)
            return Result<PaginatedList<SimpleAlertView>>.Empty;

        var take = request.Take > 50 ? 50 : request.Take;
        var lastAlertId = _idHasher.DecodeId(request.LastAlertId!);

        var query = _queries.GetSimpleAlertViewListByUserId(_session.UserId!.Value, new AlertId(lastAlertId), take);

        var paginatedListAlerts = await _context
            .UsingConnectionAsync(async conn =>
            {
                var command = new CommandDefinition(
                    query.Sql,
                    query.Parameters,
                    cancellationToken: cancellationToken);

                var result = new PaginatedList<SimpleAlertView>();
                var gridReader = await conn.QueryMultipleAsync(command);

                result.Total = await gridReader.ReadSingleAsync<int>();
                result.Results = (await gridReader.ReadAsync<SimpleAlertViewDto>())
                    .Select(dto => dto.ToSimpleAlertView(_idHasher)).ToArray();

                return result;
            });

        return paginatedListAlerts;
    }
}
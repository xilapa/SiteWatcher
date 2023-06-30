using Application.Alerts.Dtos;
using Dapper;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.DTOs;

namespace SiteWatcher.Application.Alerts.Commands.GetUserAlerts;

public class GetUserAlertsCommand : IRequest<CommandResult>, ICacheable
{
    public string? LastAlertId { get; set; }
    public int Take { get; set; } = 10;

    public string GetKey(ISession session) =>
        CacheKeys.UserAlerts(session.UserId!.Value);

    public string HashFieldName =>
        $"Take:{Take}-LastId:{LastAlertId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(60);
}

public class GetUserAlertsCommandHandler : IRequestHandler<GetUserAlertsCommand, CommandResult>
{
    private readonly IIdHasher _idHasher;
    private readonly ISession _session;
    private readonly IDapperContext _context;
    private readonly IQueries _queries;

    public GetUserAlertsCommandHandler(IIdHasher idHasher, ISession session, IDapperContext context, IQueries queries)
    {
        _idHasher = idHasher;
        _session = session;
        _context = context;
        _queries = queries;
    }

    public async Task<CommandResult> Handle(GetUserAlertsCommand request, CancellationToken cancellationToken)
    {
        if (request.Take == 0)
            return CommandResult.Empty();

        var take = request.Take > 50 ? 50 : request.Take;
        var lastAlertId = string.IsNullOrEmpty(request.LastAlertId) ? 0 : _idHasher.DecodeId(request.LastAlertId);

        var paginatedListAlerts = await _context
            .UsingConnectionAsync(async conn =>
            {
                var command = new CommandDefinition(
                        _queries.GetSimpleAlertViewListByUserId,
                        new { lastAlertId, userId = _session.UserId, take },
                        cancellationToken: cancellationToken);

                var result = new PaginatedList<SimpleAlertView>();
                var gridReader = await conn.QueryMultipleAsync(command);

                result.Total = await gridReader.ReadSingleAsync<int>();
                result.Results = (await gridReader.ReadAsync<SimpleAlertViewDto>())
                    .Select(dto => dto.ToSimpleAlertView(_idHasher)).ToArray();

                return result;
            });

        return CommandResult.FromValue(paginatedListAlerts);
    }
}
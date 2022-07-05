using Dapper;
using Domain.DTOs.Alert;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.DapperRepositories;

public class AlertDapperRepository : IAlertDapperRepository
{
    private readonly IDapperContext _dapperContext;
    private readonly IDapperQueries _dapperQueries;

    public AlertDapperRepository(IDapperContext dapperContext, IDapperQueries dapperQueries)
    {
        _dapperContext = dapperContext;
        _dapperQueries = dapperQueries;
    }

    public async Task<IEnumerable<SimpleAlertViewDto>> GetUserAlerts(UserId userId, int take, int lastAlertId,
        CancellationToken cancellationToken)
    {
        var parameters = new {lastAlertId, userId, take};
        var commandDefinition = new CommandDefinition(_dapperQueries.GetSimpleAlertViewListByUserId, parameters,
            cancellationToken: cancellationToken);
        var result = await _dapperContext.UsingConnectionAsync(conn =>
            conn.QueryAsync<SimpleAlertViewDto>(commandDefinition));
        return result;
    }
}
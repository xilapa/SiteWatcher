using System.Dynamic;
using Dapper;
using Domain.DTOs.Alert;
using Domain.DTOs.Common;
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

    public async Task<PaginatedList<SimpleAlertViewDto>> GetUserAlerts(UserId userId, int take, int lastAlertId,
        CancellationToken cancellationToken)
    {
        var parameters = new {lastAlertId, userId, take};
        var commandDefinition = new CommandDefinition(_dapperQueries.GetSimpleAlertViewListByUserId, parameters,
            cancellationToken: cancellationToken);
        var result = new PaginatedList<SimpleAlertViewDto>();
        await _dapperContext.UsingConnectionAsync(async conn =>
        {
            var gridReader = await conn.QueryMultipleAsync(commandDefinition);
            result.Total = await gridReader.ReadSingleAsync<int>();
            result.Results = await gridReader.ReadAsync<SimpleAlertViewDto>();
        });
        return result;
    }

    public async Task<AlertDetailsDto?> GetAlertDetails(int alertId, UserId userId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(_dapperQueries.GetAlertDetails, new {alertId, userId},
            cancellationToken: cancellationToken);
        return await _dapperContext.UsingConnectionAsync(conn =>
            conn.QueryFirstOrDefaultAsync<AlertDetailsDto>(commandDefinition));
    }

    public async Task<bool> DeleteUserAlert(int alertId, UserId userId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(_dapperQueries.DeleteUserAlert, new {alertId, userId},
            cancellationToken: cancellationToken);
        var affectedRows = await _dapperContext.UsingConnectionAsync(conn =>
            conn.ExecuteAsync(commandDefinition));
        return affectedRows > 0;
    }

    public async Task<List<SimpleAlertViewDto>> SearchSimpleAlerts(string[] searchTerms, UserId userId, int take,
        CancellationToken cancellationToken)
    {
        var parameters = new ExpandoObject() as IDictionary<string, object>;
        parameters[nameof(userId)] = userId;
        parameters[nameof(take)] = take;

        for (var i = 0; i < searchTerms.Length; i++)
            parameters.Add($"searchTerm{i}", $"%{searchTerms[i]}%");

        var commandDefinition = new CommandDefinition(_dapperQueries.SearchSimpleAlerts(searchTerms.Length),
            parameters, cancellationToken: cancellationToken);
        var result = await _dapperContext.UsingConnectionAsync(conn =>
            conn.QueryAsync<SimpleAlertViewDto>(commandDefinition));
        return result.AsList();
    }
}
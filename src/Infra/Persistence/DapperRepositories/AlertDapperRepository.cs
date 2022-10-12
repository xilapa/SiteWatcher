using System.Dynamic;
using Dapper;
using Domain.DTOs.Alerts;
using Domain.DTOs.Common;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.DapperRepositories;

public class AlertDapperRepository : IAlertDapperRepository
{
    private readonly IDapperContext _dapperContext;
    private readonly IDapperQueries _dapperQueries;
    private readonly IIdHasher _idHasher;

    public AlertDapperRepository(IDapperContext dapperContext, IDapperQueries dapperQueries, IIdHasher idHasher)
    {
        _dapperContext = dapperContext;
        _dapperQueries = dapperQueries;
        _idHasher = idHasher;
    }

    public async Task<PaginatedList<SimpleAlertView>> GetUserAlerts(UserId userId, int take, int lastAlertId,
        CancellationToken cancellationToken)
    {
        var parameters = new {lastAlertId, userId, take};
        var commandDefinition = new CommandDefinition(_dapperQueries.GetSimpleAlertViewListByUserId, parameters,
            cancellationToken: cancellationToken);
        var result = new PaginatedList<SimpleAlertView>();
        var simpleAlertViewDtos = Enumerable.Empty<SimpleAlertViewDto>();
        await _dapperContext.UsingConnectionAsync(async conn =>
        {
            var gridReader = await conn.QueryMultipleAsync(commandDefinition);
            result.Total = await gridReader.ReadSingleAsync<int>();
            simpleAlertViewDtos = (await gridReader.ReadAsync<SimpleAlertViewDto>()).AsList();
        });
        result.Results = simpleAlertViewDtos.Select(dto => SimpleAlertView.FromDto(dto, _idHasher));
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
        {
            parameters.Add($"searchTermWildCards{i}", $"%{searchTerms[i]}%");
            parameters.Add($"searchTerm{i}",searchTerms[i]);
        }

        var commandDefinition = new CommandDefinition(_dapperQueries.SearchSimpleAlerts(searchTerms.Length),
            parameters, cancellationToken: cancellationToken);
        var result = await _dapperContext.UsingConnectionAsync(conn =>
            conn.QueryAsync<SimpleAlertViewDto>(commandDefinition));
        return result.AsList();
    }
}
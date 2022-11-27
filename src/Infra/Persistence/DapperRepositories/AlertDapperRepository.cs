using System.Dynamic;
using Dapper;
using Domain.Alerts.DTOs;
using SiteWatcher.Application.Alerts.ViewModels;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;

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
        await _dapperContext.UsingConnectionAsync(async conn =>
        {
            var gridReader = await conn.QueryMultipleAsync(commandDefinition);
            result.Total = await gridReader.ReadSingleAsync<int>();
            result.Results = (await gridReader.ReadAsync<SimpleAlertViewDto>())
                .Select(dto => SimpleAlertView.FromDto(dto, _idHasher));
        });
        return result;
    }

    public async Task<AlertDetails?> GetAlertDetails(int alertId, UserId userId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(_dapperQueries.GetAlertDetails, new {alertId, userId},
            cancellationToken: cancellationToken);
        var alertDetailsDto = await _dapperContext.UsingConnectionAsync(conn =>
            conn.QueryFirstOrDefaultAsync<AlertDetailsDto>(commandDefinition));
        return alertDetailsDto is null ? null: AlertDetails.FromDto(alertDetailsDto, _idHasher);
    }

    public async Task<bool> DeleteUserAlert(int alertId, UserId userId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(_dapperQueries.DeleteUserAlert, new {alertId, userId},
            cancellationToken: cancellationToken);
        var affectedRows = await _dapperContext.UsingConnectionAsync(conn =>
            conn.ExecuteAsync(commandDefinition));
        return affectedRows > 0;
    }

    public async Task<IEnumerable<SimpleAlertView>> SearchSimpleAlerts(string[] searchTerms, UserId userId, int take,
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
        var alertViewDtos = await _dapperContext.UsingConnectionAsync(conn =>
            conn.QueryAsync<SimpleAlertViewDto>(commandDefinition));

        return alertViewDtos.Select(dto => SimpleAlertView.FromDto(dto, _idHasher));
    }
}
using Dapper;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Infra.DapperRepositories;

public class UserDapperRepository : IUserDapperRepository
{
    private readonly IDapperContext _dapperContext;
    private readonly IDapperQueries _dapperQueries;

    public UserDapperRepository(IDapperContext dapperContext, IDapperQueries dapperQueries)
    {
        _dapperContext = dapperContext;
        _dapperQueries = dapperQueries;
    }

    public async Task<UserViewModel?> GetUserByGoogleIdAsync(string googleId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(_dapperQueries.GetUserByGoogleId, new {googleId},
            cancellationToken: cancellationToken);
        return await _dapperContext.UsingConnectionAsync(conn =>
            conn.QuerySingleOrDefaultAsync<UserViewModel>(commandDefinition));
    }

    public Task<UserViewModel?> GetUserByIdAsync(UserId id, CancellationToken ct)
    {
        var cmmdDef = new CommandDefinition(_dapperQueries.GetUserById, new { id }, cancellationToken: ct);
        return _dapperContext.UsingConnectionAsync(conn =>
            conn.QuerySingleOrDefaultAsync<UserViewModel?>(cmmdDef));
    }

    public async Task<bool> DeleteActiveUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(_dapperQueries.DeleteActiveUserById, new {userId = userId.Value},
            cancellationToken: cancellationToken);
        var affectedRows = await _dapperContext
            .UsingConnectionAsync(conn => conn.ExecuteAsync(commandDefinition));
        return affectedRows > 0;
    }
}
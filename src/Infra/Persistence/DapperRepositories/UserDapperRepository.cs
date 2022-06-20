using Dapper;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Common;

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

    public async Task<UserViewModel> GetUserAsync(string googleId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(_dapperQueries.GetUserByGoogleId, new {googleId},
            cancellationToken: cancellationToken);
        return await _dapperContext.UsingConnectionAsync(conn =>
            conn.QuerySingleOrDefaultAsync<UserViewModel>(commandDefinition));
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
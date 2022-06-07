using Dapper;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.DapperRepositories;

public class UserDapperRepository : DapperRepository<UserViewModel>, IUserDapperRepository
{
    public UserDapperRepository(IAppSettings appSettings) : base(appSettings)
    {
    }

    public async Task<UserViewModel> GetUserAsync(string googleId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(Queries.GetUserByGoogleId, new {googleId},
            cancellationToken: cancellationToken);
        return await UsingConnectionAsync(conn =>
            conn.QuerySingleOrDefaultAsync<UserViewModel>(commandDefinition));
    }

    public async Task DeleteActiveUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        var commandDefinition = new CommandDefinition(Queries.DeleteActiveUserById, new {userId = userId.Value},
            cancellationToken: cancellationToken);
        await UsingConnectionAsync(conn => conn.ExecuteAsync(commandDefinition));
    }
}
using Dapper;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.ViewModels;

namespace SiteWatcher.Infra.DapperRepositories;

public class UserDapperRepository : DapperRepository<UserViewModel>, IUserDapperRepository
{
    public UserDapperRepository(IAppSettings appSettings) : base(appSettings)
    { }

    public async Task<UserViewModel> GetActiveUserAsync(string googleId) =>
        await UsingConnectionAsync(conn =>
            conn.QuerySingleOrDefaultAsync<UserViewModel>(Queries.GetActiveUserByGoogleId, new { googleId }));
}
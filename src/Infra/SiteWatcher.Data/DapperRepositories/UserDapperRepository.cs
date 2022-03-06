using System.Threading.Tasks;
using Dapper;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.ViewModels;

namespace SiteWatcher.Data.DapperRepositories;

public class UserDapperRepository : DapperRepository<UserViewModel>, IUserDapperRepository
{
    public UserDapperRepository(string connectionString) : base(connectionString)
    { }

    public async Task<UserViewModel> GetActiveUserAsync(string googleId)
    {
        var query = @"
                    SELECT 
	                    u.""Id"", u.""Name"", u.""Email"", u.""EmailConfirmed"", u.""Language""
                    FROM 
	                    ""Users"" AS u
                    WHERE
	                    u.""GoogleId"" = @googleId AND u.""Active"" ";

        return await UsingConnectionAsync(conn => conn.QuerySingleOrDefaultAsync<UserViewModel>(query, new { googleId }));
    }
}
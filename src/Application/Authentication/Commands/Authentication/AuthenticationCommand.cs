using Dapper;
using Domain.Authentication;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Queries;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Application.Authentication.Commands.Authentication;

public class AuthenticationCommand
{
    public string? GoogleId { get; set; }
    public string? Email { get; set; }
    public string? ProfilePicUrl { get; set; }
    public string? Name { get; set; }
    public string? Locale { get; set; }
    public string? CodeChallenge { get; set; }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(GoogleId)) return false;
        if (string.IsNullOrEmpty(Email)) return false;
        if (string.IsNullOrEmpty(CodeChallenge)) return false;
        return true;
    }

    public UserRegisterData ToRegisterData()
    {
        return new UserRegisterData
        {
            GoogleId = GoogleId!,
            Email = Email!,
            Name = Name,
            Locale = Locale
        };
    }
}

public class AuthenticationCommandHandler : IApplicationHandler
{
    private readonly IDapperContext _context;
    private readonly IQueries _queries;
    private readonly IAuthService _authService;

    public AuthenticationCommandHandler(IDapperContext context, IQueries queries, IAuthService authService)
    {
        _context = context;
        _queries = queries;
        _authService = authService;
    }

    public async ValueTask<AuthCodeResult> Handle(AuthenticationCommand request, CancellationToken ct)
    {
        if (!request.IsValid()) return new AuthCodeResult(null, ApplicationErrors.GOOGLE_AUTH_ERROR);

        var queries = _queries.GetUserByGoogleId(request.GoogleId!);
        var user = await _context.UsingConnectionAsync(conn =>
            {
                var cmd = new CommandDefinition(
                    queries.Sql,
                    queries.Parameters,
                    cancellationToken: ct);
                return conn.QueryFirstOrDefaultAsync<UserViewModel?>(cmd);
            });

        AuthenticationResult authRes = null!;
        // User exists and is active
        if (user?.Active == true)
        {
            var loginToken = _authService.GenerateLoginToken(user);
            await _authService.WhiteListToken(user.Id, loginToken);
            authRes = new AuthenticationResult(AuthTask.Login, loginToken, request.ProfilePicUrl);
        }

        // User does not exists
        if(user == null)
        {
            var registerToken = _authService.GenerateRegisterToken(request.ToRegisterData());
            authRes = new AuthenticationResult(AuthTask.Register, registerToken, request.ProfilePicUrl);
        }

        // User exists but is deactivated
        if (user?.Active == false)
            authRes = new AuthenticationResult(AuthTask.Activate, user.Id.ToString(), null);

        // store auth result
        return await _authService.StoreAuthenticationResult(authRes, request.CodeChallenge!, ct);
    }
}
using MediatR;
using SiteWatcher.Application.Authentication.Common;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;

public class GoogleAuthenticationCommand : IRequest<AuthenticationResult>
{
    public string? GoogleId { get; set; }
    public string? ProfilePicUrl { get; set; }
    public string? Email { get; set; }
    public string? Locale { get; set; }
    public string? Name { get; set; }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(GoogleId)) return false;
        if (string.IsNullOrEmpty(Email)) return false;
        if (string.IsNullOrEmpty(Locale)) return false;
        return true;
    }
}

public class GoogleAuthenticationCommandHandler : IRequestHandler<GoogleAuthenticationCommand, AuthenticationResult>
{
    private readonly IUserDapperRepository _userDapperRepository;
    private readonly IAuthService _authService;

    public GoogleAuthenticationCommandHandler(IUserDapperRepository userDapperRepository, IAuthService authService)
    {
        _userDapperRepository = userDapperRepository;
        _authService = authService;
    }

    public async Task<AuthenticationResult> Handle(GoogleAuthenticationCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsValid())
            return new AuthenticationResult(string.Empty, error: ApplicationErrors.GOOGLE_AUTH_ERROR);

        var user = await _userDapperRepository.GetUserByGoogleIdAsync(request.GoogleId!, cancellationToken);
        string token;

        // User exists and is active
        if (user?.Active is true)
        {
            token = await _authService.CreateLoginAuthSession(user, request.ProfilePicUrl);
            return new AuthenticationResult(token);
        }

        // User does not exists
        if (user is null)
        {
            token = await _authService.CreateRegisterAuthSession(request.GoogleId!, request.Name!, request.Email!, request.Locale!, request.ProfilePicUrl);
            return new AuthenticationResult(token);
        }

        // User exists but is deactivated
        token = await _authService.CreateActivateAuthSession(user);
        return new AuthenticationResult(token);
    }
}
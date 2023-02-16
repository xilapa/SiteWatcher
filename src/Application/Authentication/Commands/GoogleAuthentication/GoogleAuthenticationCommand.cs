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
        if(string.IsNullOrEmpty(GoogleId)) return false;
        if(string.IsNullOrEmpty(Email)) return false;
        if(string.IsNullOrEmpty(Locale)) return false;
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
            return new AuthenticationResult(AuthTask.Error, string.Empty, message: ApplicationErrors.GOOGLE_AUTH_ERROR);

        var user = await _userDapperRepository.GetUserByGoogleIdAsync(request.GoogleId!, cancellationToken);

        // User exists and is active
        if (user?.Active is true)
        {
            var loginToken = _authService.GenerateLoginToken(user);
            await _authService.WhiteListToken(user.Id, loginToken);
            return new AuthenticationResult(AuthTask.Login, loginToken, request.ProfilePicUrl);
        }

        // User does not exists
        if(user is null)
        {
            var registerToken = _authService.GenerateRegisterToken(request.GoogleId!, request.Name!, request.Email!, request.Locale!);
            return new AuthenticationResult(AuthTask.Register, registerToken, request.ProfilePicUrl);
        }

        // User exists but is deactivated
        return new AuthenticationResult(AuthTask.Activate, user.Id.Value.ToString());
    }
}
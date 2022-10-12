using MediatR;
using SiteWatcher.Application.Authentication.Common;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;

public class GoogleAuthenticationCommand : IRequest<CommandResult>
{
    public string? State { get; set; }
    public string? Code { get; set; }
    public string? Scope { get; set; }
}

public class GoogleAuthenticationCommandHandler : IRequestHandler<GoogleAuthenticationCommand, CommandResult>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserDapperRepository _userDapperRepository;
    private readonly IAuthService _authService;

    public GoogleAuthenticationCommandHandler(IGoogleAuthService googleAuthService, IUserDapperRepository userDapperRepository,
    IAuthService authService)
    {
        _googleAuthService = googleAuthService;
        _userDapperRepository = userDapperRepository;
        _authService = authService;
    }

    public async Task<CommandResult> Handle(GoogleAuthenticationCommand request, CancellationToken cancellationToken)
    {
        var tokenResult = await _googleAuthService.ExchangeCode(request.Code!, cancellationToken);
        if(!tokenResult.Success)
            return CommandResult.FromError(ApplicationErrors.GOOGLE_AUTH_ERROR);

        var user = await _userDapperRepository.GetUserAsync(tokenResult.GoogleId, cancellationToken);

        // User exists and is active
        if (user?.Active is true)
        {
            var loginToken = _authService.GenerateLoginToken(user);
            await _authService.WhiteListToken(user.Id, loginToken);
            return CommandResult
                .FromValue(new AuthenticationResult(EAuthTask.Login, loginToken, tokenResult.ProfilePicUrl));
        }

        // User does not exists
        if(user is null)
        {
            var registerToken = _authService.GenerateRegisterToken(tokenResult.Claims, tokenResult.GoogleId);
            return CommandResult
                .FromValue(new AuthenticationResult(EAuthTask.Register, registerToken, tokenResult.ProfilePicUrl));
        }

        // User exists but is deactivated
        return CommandResult
            .FromValue(new AuthenticationResult(EAuthTask.Activate, user.Id.Value.ToString(), null));
    }
}
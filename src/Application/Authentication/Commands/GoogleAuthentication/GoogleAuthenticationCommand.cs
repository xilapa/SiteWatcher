using MediatR;
using SiteWatcher.Application.Authentication.Common;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;

public class GoogleAuthenticationCommand : Validable<GoogleAuthenticationCommand>, IRequest<ICommandResult<AuthenticationResult>>
{
    public string? State { get; set; }
    public string? Code { get; set; }
    public string? Scope { get; set; }
}

public class GoogleAuthenticationCommandHandler : IRequestHandler<GoogleAuthenticationCommand,
        ICommandResult<AuthenticationResult>>
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

    public async Task<ICommandResult<AuthenticationResult>> Handle(GoogleAuthenticationCommand request,
        CancellationToken cancellationToken)
    {
        var appResult = new CommandResult<AuthenticationResult>();
        var tokenResult = await _googleAuthService.ExchangeCode(request.Code!, cancellationToken);
        if(!tokenResult.Success)
            return appResult.WithError(ApplicationErrors.GOOGLE_AUTH_ERROR);

        var user = await _userDapperRepository.GetUserAsync(tokenResult.GoogleId, cancellationToken);

        if(user.Id.Equals(UserId.Empty))
        {
            var registerToken = _authService.GenerateRegisterToken(tokenResult.Claims, tokenResult.GoogleId);
            appResult.WithValue(new AuthenticationResult(EAuthTask.Register, registerToken, tokenResult.ProfilePicUrl));
        }
        else if(user.Active && !user.Id.Equals(UserId.Empty))
        {
            var loginToken = _authService.GenerateLoginToken(user);
            await _authService.WhiteListToken(user.Id, loginToken);
            appResult.WithValue(new AuthenticationResult(EAuthTask.Login, loginToken, tokenResult.ProfilePicUrl));
        }
        else if (!user.Active && !user.Id.Equals(UserId.Empty))
        {
            appResult.WithValue(new AuthenticationResult(EAuthTask.Activate, user.Id.Value.ToString(), null));
        }

        return appResult;
    }
}
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Errors;

namespace SiteWatcher.Application.Users.Commands.ReactivateAccount;

public class ReactivateAccountCommand
{
    public string? Token { get; set; }
}

public class ReactivateAccountCommandHandler : IApplicationHandler
{
    private readonly IAuthService _authService;
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;

    public ReactivateAccountCommandHandler(IAuthService authService, ISiteWatcherContext context, ISession session)
    {
        _authService = authService;
        _context = context;
        _session = session;
    }

    public async ValueTask<Result> Handle(ReactivateAccountCommand request, CancellationToken cancellationToken)
    {
        if(request.Token == null)
            return ReturnError();

        var userId = await _authService.GetUserIdFromConfirmationToken(request.Token);
        if(userId is null)
            return ReturnError();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.Active, cancellationToken);
        if(user is null)
            return ReturnError();

        var success = user.ReactivateAccount(request.Token, _session.Now);
        if(!success)
            return ReturnError();

        await _context.SaveChangesAsync(CancellationToken.None);
        return Result.Empty;
    }

    private static Error ReturnError() =>
        Error.Validation(ApplicationErrors.ValueIsInvalid(nameof(ReactivateAccountCommand.Token)));
}
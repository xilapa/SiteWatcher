using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Errors;

namespace SiteWatcher.Application.Users.Commands.ConfirmEmail;

public class ConfirmEmailCommand
{
    public string? Token { get; set; }
}

public class ConfirmEmailCommandHandler : IApplicationHandler
{
    private readonly IAuthService _authservice;
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;

    public ConfirmEmailCommandHandler(IAuthService authservice, ISiteWatcherContext context, ISession session)
    {
        _authservice = authservice;
        _context = context;
        _session = session;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (request.Token == null)
            return ReturnError();

        var userId = await _authservice.GetUserIdFromConfirmationToken(request.Token);
        if (userId is null)
            return ReturnError();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.Active && !u.EmailConfirmed, cancellationToken);
        if (user is null)
            return ReturnError();

        var success = user.ConfirmEmail(request.Token, _session.Now);
        if (!success)
            return ReturnError();

        await _context.SaveChangesAsync(CancellationToken.None);
        return Result.Empty;
    }

    private static Error ReturnError() =>
        Error.Validation(ApplicationErrors.ValueIsInvalid(nameof(ConfirmEmailCommand.Token)));
}
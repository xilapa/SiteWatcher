using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Users.Commands.ActivateAccount;

public class SendReactivateAccountEmailCommand
{
    public UserId UserId { get; set; }
}

public class SendReactivateAccountEmailCommandHandler : BaseHandler<SendReactivateAccountEmailCommand, Result>
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;

    public SendReactivateAccountEmailCommandHandler(ISiteWatcherContext context, ISession session,
        IValidator<SendReactivateAccountEmailCommand> validator) : base(validator)
    {
        _context = context;
        _session = session;
    }

    protected override async Task<Result> HandleCommand(SendReactivateAccountEmailCommand command, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId && !u.Active, ct);
        if (user is null)
            return Result.Empty;

        user.GenerateReactivationToken(_session.Now);
        await _context.SaveChangesAsync(CancellationToken.None);
        return Result.Empty;
    }
}
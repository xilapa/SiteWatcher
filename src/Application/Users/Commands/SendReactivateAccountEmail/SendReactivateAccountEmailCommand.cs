using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Services;
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
    private readonly IPublisher _publisher;

    public SendReactivateAccountEmailCommandHandler(ISiteWatcherContext context, ISession session,
        IValidator<SendReactivateAccountEmailCommand> validator, IPublisher publisher) : base(validator)
    {
        _context = context;
        _session = session;
        _publisher = publisher;
    }

    protected override async Task<Result> HandleCommand(SendReactivateAccountEmailCommand command, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId && !u.Active, ct);
        if (user is null)
            return Result.Empty;

        var userReactivationTokenGeneratedMessage = user.GenerateReactivationToken(_session.Now);
        if (userReactivationTokenGeneratedMessage != null)
            await _publisher.PublishAsync(userReactivationTokenGeneratedMessage, ct);

        await _context.SaveChangesAsync(CancellationToken.None);
        return Result.Empty;
    }
}
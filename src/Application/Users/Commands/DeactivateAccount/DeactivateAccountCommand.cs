﻿using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.EventHandlers;
using SiteWatcher.Domain.Authentication;

namespace SiteWatcher.Application.Users.Commands.DeactivateAccount;

public class DeactivateAccountCommandHandler : IApplicationHandler
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;
    private readonly UserUpdatedEventHandler _userUpdatedEventHandler;

    public DeactivateAccountCommandHandler(ISiteWatcherContext context, ISession session,
        UserUpdatedEventHandler userUpdatedEventHandler)
    {
        _context = context;
        _session = session;
        _userUpdatedEventHandler = userUpdatedEventHandler;
    }

    public async Task Handle(CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active, ct);
        if (user is null)
            return;

        var userUpdatedEvent = user.Deactivate(_session.Now);
        await _context.SaveChangesAsync(ct);
        await _userUpdatedEventHandler.Handle(userUpdatedEvent, ct);
    }
}
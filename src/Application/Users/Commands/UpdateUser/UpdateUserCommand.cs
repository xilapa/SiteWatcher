using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Application.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<CommandResult>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Language Language { get; set; }
    public Theme Theme { get; set; }

    public UpdateUserInput ToInputModel() => new (Name!, Email!, Language, Theme);
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, CommandResult>
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;
    private readonly ICache _cache;

    public UpdateUserCommandHandler(ISiteWatcherContext context, ISession session, ICache cache)
    {
        _context = context;
        _session = session;
        _cache = cache;
    }

    public async Task<CommandResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active, cancellationToken);
        if (user is null)
            return CommandResult.FromError(ApplicationErrors.USER_DO_NOT_EXIST);

        user.Update(request.ToInputModel(), _session.Now);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.DeleteKeyAsync(CacheKeys.UserInfo(_session.UserId!.Value));

        return CommandResult.FromValue(new UpdateUserResult(new UserViewModel(user), !user.EmailConfirmed));
    }
}
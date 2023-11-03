using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.EventHandlers;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Errors;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Application.Users.Commands.UpdateUser;

public class UpdateUserCommand
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Language Language { get; set; }
    public Theme Theme { get; set; }

    public UpdateUserInput ToInputModel() => new(Name!, Email!, Language, Theme);
}

public class UpdateUserCommandHandler : BaseHandler<UpdateUserCommand, Result<UpdateUserResult>>
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;
    private readonly UserUpdatedEventHandler _userUpdatedEventHandler;
    private readonly IPublisher _publisher;

    public UpdateUserCommandHandler(ISiteWatcherContext context, ISession session,
        IValidator<UpdateUserCommand> validator, UserUpdatedEventHandler userUpdatedEventHandler, IPublisher publisher)
        : base(validator)
    {
        _context = context;
        _session = session;
        _userUpdatedEventHandler = userUpdatedEventHandler;
        _publisher = publisher;
    }

    protected override async Task<Result<UpdateUserResult>> HandleCommand(UpdateUserCommand command,
        CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active, ct);
        if (user is null)
            return Error.Validation(ApplicationErrors.USER_DO_NOT_EXIST);

        var (userUpdatedEvent, tokenGeneratedMessage) = user.Update(command.ToInputModel(), _session.Now);
        if (tokenGeneratedMessage != null)
            await _publisher.PublishAsync(tokenGeneratedMessage, ct);

        await _context.SaveChangesAsync(ct);
        await _userUpdatedEventHandler.Handle(userUpdatedEvent, ct);

        return new UpdateUserResult(new UserViewModel(user), !user.EmailConfirmed);
    }
}
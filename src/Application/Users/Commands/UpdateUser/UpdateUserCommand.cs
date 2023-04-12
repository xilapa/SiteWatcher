using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.Repositories;

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
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISession _session;
    private readonly ICache _cache;

    public UpdateUserCommandHandler(IUserRepository userRepo, IUnitOfWork uow, ISession session, ICache cache)
    {
        _userRepo = userRepo;
        _uow = uow;
        _session = session;
        _cache = cache;
    }

    public async Task<CommandResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetAsync(u => u.Id == _session.UserId && u.Active, cancellationToken);
        if (user is null)
            return CommandResult.FromError(ApplicationErrors.USER_DO_NOT_EXIST);

        user.Update(request.ToInputModel(), _session.Now);
        await _uow.SaveChangesAsync(cancellationToken);
        await _cache.DeleteKeyAsync(CacheKeys.UserInfo(_session.UserId!.Value));

        return CommandResult.FromValue(new UpdateUserResult(new UserViewModel(user), !user.EmailConfirmed));
    }
}
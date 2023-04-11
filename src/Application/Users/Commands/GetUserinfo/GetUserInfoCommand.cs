using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Application.Users.Commands.GetUserinfo;

public sealed class GetUserInfoCommand : IRequest<UserViewModel?>, ICacheable
{
    public TimeSpan Expiration => TimeSpan.FromMinutes(15);
    public string HashFieldName => string.Empty;

    public string GetKey(ISession session) =>
        CacheKeys.UserInfo(session.UserId!.Value);
}

public sealed class GetUserInfoCommandHandler : IRequestHandler<GetUserInfoCommand, UserViewModel?>
{
    private readonly IUserDapperRepository _userRepo;
    private readonly ISession _session;

    public GetUserInfoCommandHandler(IUserDapperRepository userRepo, ISession session)
    {
        _userRepo = userRepo;
        _session = session;
    }

    public Task<UserViewModel?> Handle(GetUserInfoCommand request, CancellationToken ct)
    {
        if (UserId.Empty.Equals(_session.UserId))
            return Task.FromResult<UserViewModel?>(null);

        return _userRepo.GetUserByIdAsync(_session.UserId!.Value, ct);
    }
}
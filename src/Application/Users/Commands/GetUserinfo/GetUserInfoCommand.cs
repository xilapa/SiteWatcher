using MediatR;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Application.Users.Commands.GetUserinfo;

public sealed class GetUserInfoCommand : IRequest<UserViewModel?>
{
    public UserId UserId { get; set; }
}

public sealed class GetUserInfoCommandHandler : IRequestHandler<GetUserInfoCommand, UserViewModel?>
{
    private readonly IUserDapperRepository _userRepo;

    public GetUserInfoCommandHandler(IUserDapperRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public Task<UserViewModel?> Handle(GetUserInfoCommand request, CancellationToken ct)
    {
        if (UserId.Empty.Equals(request.UserId))
            return Task.FromResult<UserViewModel?>(null);

        return _userRepo.GetUserByIdAsync(request.UserId, ct);
    }
}
using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Users.Commands.ActivateAccount;

public class SendReactivateAccountEmailCommand : IRequest
{
    public UserId UserId { get; set; }
}

public class SendReactivateAccountEmailCommandHandler : IRequestHandler<SendReactivateAccountEmailCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ISession _session;
    private readonly IUnitOfWork _uow;

    public SendReactivateAccountEmailCommandHandler(IUserRepository userRepository, ISession session, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _session = session;
        _uow = uow;
    }

    public async Task<Unit> Handle(SendReactivateAccountEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(u => u.Id == request.UserId && !u.Active, cancellationToken);
        if(user is null)
            return Unit.Value;

        user.GenerateUserActivationToken(_session.Now);
        await _uow.SaveChangesAsync(CancellationToken.None);
        return Unit.Value;
    }
}
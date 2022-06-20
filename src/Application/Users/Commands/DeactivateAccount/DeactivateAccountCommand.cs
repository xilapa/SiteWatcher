using MediatR;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Users.Commands.DeactivateAccount;

public class DeactivateAccountCommand : IRequest
{ }

public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    private readonly ISession _session;

    public DeactivateAccountCommandHandler(IUserRepository userRepository, IUnitOfWork uow, ISession session)
    {
        _userRepository = userRepository;
        _uow = uow;
        _session = session;
    }

    public async Task<Unit> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await  _userRepository
            .GetAsync(u => u.Id == _session.UserId && u.Active, cancellationToken);
        if (user is null)
            return Unit.Value;

        user.Deactivate(_session.Now);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
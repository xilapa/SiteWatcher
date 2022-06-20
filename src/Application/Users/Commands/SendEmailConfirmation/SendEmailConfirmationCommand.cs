using MediatR;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Users.Commands.SendEmailConfirmation;

public class SendEmailConfirmationCommand : IRequest
{ }

public class SendEmailConfirmationCommandHandler : IRequestHandler<SendEmailConfirmationCommand>
{
    private readonly ISession _session;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;

    public SendEmailConfirmationCommandHandler(ISession session, IUserRepository userRepository, IUnitOfWork uow)
    {
        _session = session;
        _userRepository = userRepository;
        _uow = uow;
    }

    public async Task<Unit> Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(u => u.Id == _session.UserId && u.Active, cancellationToken);
        if(user is null)
            return Unit.Value;

        user.GenerateEmailConfirmationToken(_session.Now);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
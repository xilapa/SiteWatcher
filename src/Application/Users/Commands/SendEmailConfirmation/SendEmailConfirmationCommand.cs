using MediatR;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Users.Commands.SendEmailConfirmation;

public class SendEmailConfirmationCommand : IRequest
{ }

public class SendEmailConfirmationCommandHandler : IRequestHandler<SendEmailConfirmationCommand>
{
    private readonly ISessao _sessao;
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _uow;

    public SendEmailConfirmationCommandHandler(ISessao sessao, IUserRepository userRepository, IUnityOfWork uow)
    {
        _sessao = sessao;
        _userRepository = userRepository;
        _uow = uow;
    }

    public async Task<Unit> Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(u => u.Id == _sessao.UserId && u.Active, cancellationToken);
        if(user is null)
            return Unit.Value;

        user.GenerateEmailConfirmationToken(_sessao.Now);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
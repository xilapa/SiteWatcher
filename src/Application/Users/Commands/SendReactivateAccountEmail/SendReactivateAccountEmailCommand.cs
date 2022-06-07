using MediatR;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Users.Commands.ActivateAccount;

public class SendReactivateAccountEmailCommand : Validable<SendReactivateAccountEmailCommand>, IRequest
{
    public UserId UserId { get; set; }
}

public class SendReactivateAccountEmailCommandHandler : IRequestHandler<SendReactivateAccountEmailCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ISessao _sessao;
    private readonly IUnityOfWork _uow;

    public SendReactivateAccountEmailCommandHandler(IUserRepository userRepository, ISessao sessao, IUnityOfWork uow)
    {
        _userRepository = userRepository;
        _sessao = sessao;
        _uow = uow;
    }

    public async Task<Unit> Handle(SendReactivateAccountEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(u => u.Id == request.UserId && !u.Active, cancellationToken);
        user?.GenerateUserActivationToken(_sessao.Now);
        await _uow.SaveChangesAsync(CancellationToken.None);
        return Unit.Value;
    }
}
using AutoMapper;
using MediatR;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Users.Commands.DeactivateAccount;

public class DeactivateAccountCommand : IRequest
{ }

public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _uow;
    private readonly ISessao _sessao;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public DeactivateAccountCommandHandler(IUserRepository userRepository,
        IUnityOfWork uow, ISessao sessao, IMediator mediator, IMapper mapper)
    {
        _userRepository = userRepository;
        _uow = uow;
        _sessao = sessao;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await  _userRepository.GetAsync(u => u.Id == _sessao.UserId && u.Active, cancellationToken);
        if (user is null)
            return Unit.Value;

        user.Deactivate(_sessao.Now);
        await _uow.SaveChangesAsync();

        await _mediator.Publish(_mapper.Map<AccountDeactivatedNotification>(_sessao));
        return Unit.Value;
    }
}
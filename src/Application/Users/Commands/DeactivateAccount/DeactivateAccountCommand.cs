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
    private readonly ISession _session;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public DeactivateAccountCommandHandler(IUserRepository userRepository,
        IUnityOfWork uow, ISession session, IMediator mediator, IMapper mapper)
    {
        _userRepository = userRepository;
        _uow = uow;
        _session = session;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await  _userRepository.GetAsync(u => u.Id == _session.UserId && u.Active, cancellationToken);
        if (user is null)
            return Unit.Value;

        user.Deactivate(_session.Now);
        await _uow.SaveChangesAsync();

        await _mediator.Publish(_mapper.Map<AccountDeactivatedNotification>(_session));
        return Unit.Value;
    }
}
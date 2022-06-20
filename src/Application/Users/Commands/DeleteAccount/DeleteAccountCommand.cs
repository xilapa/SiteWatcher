using AutoMapper;
using Domain.Events;
using MediatR;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Users.Commands.DeleteUser;

public class DeleteAccountCommand : IRequest
{ }

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IUserDapperRepository _userRepository;
    private readonly ISession _session;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public DeleteAccountCommandHandler(IUserDapperRepository userRepository, ISession session, IMediator mediator, IMapper mapper)
    {
        _userRepository = userRepository;
        _session = session;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userDeleted = await _userRepository.DeleteActiveUserAsync(_session.UserId!.Value, cancellationToken);
        if(userDeleted)
            await _mediator.Publish(_mapper.Map<AccountDeletedEvent>(_session), CancellationToken.None);
        return Unit.Value;
    }
}
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
    private readonly ISessao _sessao;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public DeleteAccountCommandHandler(IUserDapperRepository userRepository, ISessao sessao, IMediator mediator, IMapper mapper)
    {
        _userRepository = userRepository;
        _sessao = sessao;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        await _userRepository.DeleteActiveUserAsync(_sessao.UserId!.Value, cancellationToken);
        await _mediator.Publish(_mapper.Map<AccountDeletedEvent>(_sessao), CancellationToken.None);
        return Unit.Value;
    }
}
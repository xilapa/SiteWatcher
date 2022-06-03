using AutoMapper;
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
    private readonly IUserDapperRepository _userDapperRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SendReactivateAccountEmailCommandHandler(IUserDapperRepository userDapperRepository, IMediator mediator, IMapper mapper)
    {
        _userDapperRepository = userDapperRepository;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(SendReactivateAccountEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userDapperRepository.GetInactiveUserAsync(request.UserId, cancellationToken);
        if (Guid.Empty.Equals(user.UserId.Value))
            return Unit.Value;

        await _mediator.Publish(_mapper.Map<AccountReactivationEmailNotification>(user), cancellationToken);
        return Unit.Value;
    }
}
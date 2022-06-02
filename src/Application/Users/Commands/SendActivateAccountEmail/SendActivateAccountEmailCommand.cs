using AutoMapper;
using MediatR;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Users.Commands.ActivateAccount;

public class SendActivateAccountEmailCommand : Validable<SendActivateAccountEmailCommand>, IRequest
{
    public UserId UserId { get; set; }
}

public class SendActivateAccountEmailCommandHandler : IRequestHandler<SendActivateAccountEmailCommand>
{
    private readonly IUserDapperRepository _userDapperRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SendActivateAccountEmailCommandHandler(IUserDapperRepository userDapperRepository, IMediator mediator, IMapper mapper)
    {
        _userDapperRepository = userDapperRepository;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(SendActivateAccountEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userDapperRepository.GetInactiveUserAsync(request.UserId, cancellationToken);
        if (Guid.Empty.Equals(user.UserId.Value))
            return Unit.Value;

        await _mediator.Publish(_mapper.Map<AccountActivationNotification>(user), cancellationToken);
        return Unit.Value;
    }
}
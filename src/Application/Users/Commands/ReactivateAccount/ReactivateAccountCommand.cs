﻿using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Users.Commands.ReactivateAccount;

public class ReactivateAccountCommand : IRequest<ICommandResult<object>>
{
    public string Token { get; set; }
}

public class ReactivateAccountCommandHandler : IRequestHandler<ReactivateAccountCommand, ICommandResult<object>>
{
    private readonly IAuthService _authservice;
    private readonly IUserRepository _userRepository;
    private readonly ISessao _sessao;
    private readonly IUnityOfWork _uow;

    public ReactivateAccountCommandHandler(IAuthService authservice, IUserRepository userRepository, ISessao sessao, IUnityOfWork uow)
    {
        _authservice = authservice;
        _userRepository = userRepository;
        _sessao = sessao;
        _uow = uow;
    }

    public async Task<ICommandResult<object>> Handle(ReactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var result = new CommandResult<object>();
        var userId = await _authservice.GetUserIdFromConfirmationToken(request.Token);
        if(userId is null)
            return result.WithError(ApplicationErrors.INVALID_TOKEN);

        var user = await _userRepository.GetAsync(u => u.Id == userId && !u.Active, cancellationToken);
        if(user is null)
            return result.WithError(ApplicationErrors.INVALID_TOKEN);

        var success = user!.ReactivateAccount(request.Token, _sessao.Now);
        if(!success)
            result.SetError(ApplicationErrors.INVALID_TOKEN);

        await _uow.SaveChangesAsync(CancellationToken.None);
        return result;
    }
}
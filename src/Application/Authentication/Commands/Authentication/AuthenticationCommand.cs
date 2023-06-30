﻿using Dapper;
using Domain.Authentication;
using MediatR;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Application.Authentication.Commands.Authentication;

public class AuthenticationCommand : IRequest<AuthCodeResult>
{
    public string? GoogleId { get; set; }
    public string? Email { get; set; }
    public string? ProfilePicUrl { get; set; }
    public string? Name { get; set; }
    public string? Locale { get; set; }
    public string? CodeChallenge { get; set; }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(GoogleId)) return false;
        if (string.IsNullOrEmpty(Email)) return false;
        if (string.IsNullOrEmpty(CodeChallenge)) return false;
        return true;
    }

    public UserRegisterData ToRegisterData()
    {
        return new UserRegisterData
        {
            GoogleId = GoogleId!,
            Email = Email!,
            Name = Name,
            Locale = Locale
        };
    }
}

public class AuthenticationCommandHandler : IRequestHandler<AuthenticationCommand, AuthCodeResult>
{
    private readonly IDapperContext _context;
    private readonly IQueries _queries;
    private readonly IAuthService _authService;

    public AuthenticationCommandHandler(IDapperContext context, IQueries queries, IAuthService authService)
    {
        _context = context;
        _queries = queries;
        _authService = authService;
    }

    public async Task<AuthCodeResult> Handle(AuthenticationCommand request, CancellationToken ct)
    {
        if (!request.IsValid()) return new AuthCodeResult(null, ApplicationErrors.GOOGLE_AUTH_ERROR);

        var user = await _context.UsingConnectionAsync(conn =>
            {
                var cmd = new CommandDefinition(
                    _queries.GetUserByGoogleId,
                    new {googleId  = request.GoogleId},
                    cancellationToken: ct);
                return conn.QueryFirstOrDefaultAsync<UserViewModel?>(cmd);
            });

        AuthenticationResult authRes = null!;
        // User exists and is active
        if (user?.Active == true)
        {
            var loginToken = _authService.GenerateLoginToken(user);
            await _authService.WhiteListToken(user.Id, loginToken);
            authRes = new AuthenticationResult(AuthTask.Login, loginToken, request.ProfilePicUrl);
        }

        // User does not exists
        if(user == null)
        {
            var registerToken = _authService.GenerateRegisterToken(request.ToRegisterData());
            authRes = new AuthenticationResult(AuthTask.Register, registerToken, request.ProfilePicUrl);
        }

        // User exists but is deactivated
        if (user?.Active == false)
            authRes = new AuthenticationResult(AuthTask.Activate, user.Id.ToString(), null);

        // store auth result
        return await _authService.StoreAuthenticationResult(authRes, request.CodeChallenge!, ct);
    }
}
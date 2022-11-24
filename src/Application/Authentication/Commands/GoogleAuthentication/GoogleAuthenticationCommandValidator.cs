using FluentValidation;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;

public class GoogleAuthenticationCommandValidator : AbstractValidator<GoogleAuthenticationCommand>
{
    private readonly IGoogleSettings _googleSettings;
    private readonly ICache _cache;
    private readonly ILogger<GoogleAuthenticationCommandValidator> _logger;

    public GoogleAuthenticationCommandValidator(IGoogleSettings googleSettings, ICache cache,
        ILogger<GoogleAuthenticationCommandValidator> logger)
    {
        _googleSettings = googleSettings;
        _cache = cache;
        _logger = logger;
        RuleFor(cmmd => cmmd.Scope)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.GOOGLE_AUTH_ERROR)
            .Must(scope => !IsMissingScope(googleSettings.Scopes, scope))
            .WithMessage(ApplicationErrors.GOOGLE_AUTH_ERROR);

        RuleFor(cmmd => cmmd.State)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.GOOGLE_AUTH_ERROR)
            .MustAsync((state, _) => StateMatch(state!))
            .WithMessage(ApplicationErrors.GOOGLE_AUTH_ERROR);

        RuleFor(cmmd => cmmd.Code)
            .NotEmpty()
            .WithMessage(ApplicationErrors.GOOGLE_AUTH_ERROR);
    }

    private bool IsMissingScope(string defaultScopes, string? scopesToBeChecked)
    {
        var scopesToBeCheckedSpan = scopesToBeChecked.AsSpan();
        var scopeSpan = defaultScopes.AsSpan();

        var crrIdx = 0;
        var sepIdx = 0;
        var sepDist = scopeSpan.IndexOf(' ');

        while (true)
        {
            var scope = scopeSpan.Slice(crrIdx, sepDist);

            if (scopesToBeCheckedSpan.IndexOf(scope) == -1)
            {
                _logger.LogError("Invalid Scopes passed at {Date} \n Scopes: {Scopes}", DateTime.UtcNow,
                    scopesToBeChecked);
                return true;
            }

            if (sepIdx == -1)
                break;

            crrIdx = crrIdx + sepDist + 1;
            sepIdx = scopeSpan[crrIdx..].IndexOf(' ');
            sepDist = sepIdx == -1 ? scopeSpan.Length - crrIdx : sepIdx;
        }

        return false;
    }

    private async Task<bool> StateMatch(string stateKey)
    {
        var storedState = await _cache.GetAndRemoveBytesAsync(stateKey);
        if (_googleSettings.StateValue.SequenceEqual(storedState ?? Array.Empty<byte>()))
            return true;

        _logger.LogError(
            "Invalid State passed at {Date} \n State from request: {State} \nCached state {CachedState}",
            DateTime.UtcNow, stateKey, storedState);
        return false;
    }
}
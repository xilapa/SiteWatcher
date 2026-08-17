using SiteWatcher.Domain.Authentication;

namespace Domain.Authentication;

public sealed class AuthenticationData
{
    public AuthenticationData(AuthenticationResult authenticationResult, string codeChallenge, string redirectOrigin)
    {
        AuthenticationResult = authenticationResult;
        CodeChallenge = codeChallenge;
        RedirectOrigin = redirectOrigin;
    }

    public AuthenticationResult AuthenticationResult { get; set; }
    public string CodeChallenge { get; set; }
    public string RedirectOrigin { get; }
}
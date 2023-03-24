using Domain.Common.Services;
using Microsoft.AspNetCore.DataProtection;

namespace SiteWatcher.Infra.DataProtection;

public sealed class DataProtectorService : IDataProtectorService
{
    private readonly ITimeLimitedDataProtector _protector;

    public DataProtectorService(IDataProtectionProvider protectionProvider)
    {
        _protector = protectionProvider.CreateProtector("SiteWatcher").ToTimeLimitedDataProtector();
    }

    public string Protect(string plaintext, TimeSpan lifetime) =>
        _protector.Protect(plaintext, lifetime);

    public string Unprotect(string protectedData)
    {
        var unprotected = string.Empty;
        try
        {
            unprotected = _protector.Unprotect(protectedData);
        }
        catch
        {
            // swallow unprotect exceptions
        }

        return unprotected;
    }
}
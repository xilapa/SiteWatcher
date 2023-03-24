namespace Domain.Common.Services;

public interface IDataProtectorService
{
    public string Protect(string plaintext, TimeSpan lifetime);
    public string Unprotect(string protectedData);
}
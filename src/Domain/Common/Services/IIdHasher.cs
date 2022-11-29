namespace SiteWatcher.Common.Services;

public interface IIdHasher
{
    string HashId(int id);
    int DecodeId(string hashedId);
}
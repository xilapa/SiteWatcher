using HashidsNet;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Infra.IdHasher;

public class IdHasher : IIdHasher
{
    private readonly Hashids _hashids;

    public IdHasher(IAppSettings appSettings)
    {
        _hashids = new Hashids(appSettings.IdHasherSalt, appSettings.MinimumHashedIdLength);
    }

    public string HashId(int id) =>
        _hashids.Encode(id);
}
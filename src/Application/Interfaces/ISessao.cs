using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface ISessao
{
    public DateTime Now { get; }
    public UserId? UserId { get;}
    public string? Email { get;}
    public string? GoogleId { get; }
    public string? UserName { get; }
    public ELanguage? Language { get; }
    public string AuthTokenPayload { get; }
}
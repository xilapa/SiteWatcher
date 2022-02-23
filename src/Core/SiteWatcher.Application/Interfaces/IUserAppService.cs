using System.Threading.Tasks;
using SiteWatcher.Application.DTOs.InputModels;
using SiteWatcher.Application.DTOs.Metadata;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Interfaces;

public interface IUserAppService
{
    Task<ApplicationResponseOf<ESubscriptionResult>> Subscribe(UserSubscribeIM userSubscribeIM);
}

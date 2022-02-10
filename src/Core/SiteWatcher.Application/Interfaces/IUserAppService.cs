using System.Threading.Tasks;
using SiteWatcher.Application.DTOS.InputModels;
using SiteWatcher.Application.DTOS.Metadata;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Interfaces;

public interface IUserAppService
{
    Task<ApplicationResponseOf<ESubscriptionResult>> Subscribe(UserSubscribeIM userSubscribeIM);
}

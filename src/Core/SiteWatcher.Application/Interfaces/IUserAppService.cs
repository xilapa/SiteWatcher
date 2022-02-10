using System.Threading.Tasks;
using AFA.Application.DTOS.InputModels;
using AFA.Application.DTOS.Metadata;
using AFA.Domain.Enums;

namespace AFA.Application.Interfaces;

public interface IUserAppService
{
    Task<ApplicationResponseOf<ESubscriptionResult>> Subscribe(UserSubscribeIM userSubscribeIM);
}

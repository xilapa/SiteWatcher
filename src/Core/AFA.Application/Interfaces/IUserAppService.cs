using System.Threading.Tasks;
using AFA.Application.DTOS.InputModels;
using AFA.Application.DTOS.Metadata;

namespace AFA.Application.Interfaces;

public interface IUserAppService
{
    Task<ApplicationResponse> Subscribe(UserSubscribeIM userSubscribeIM);
}

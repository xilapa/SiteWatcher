using System.Threading.Tasks;
using AFA.Domain.DTOS.InputModels;

namespace AFA.Application.Interfaces;

public interface IUserAppService
{
    Task Subscribe(UserSubscribeIM userSubscribe);
}

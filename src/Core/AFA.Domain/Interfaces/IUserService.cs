using System;
using System.Threading.Tasks;
using AFA.Domain.Entities;
using AFA.Domain.Enums;

namespace AFA.Domain.Interfaces;

public interface IUserService
{
    Task<User> CreateUser(string name, string email);
    Task<ESubscriptionResult> SubscribeUser(Guid userId);
}
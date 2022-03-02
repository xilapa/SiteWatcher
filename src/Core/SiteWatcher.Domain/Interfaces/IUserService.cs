using System;
using System.Threading.Tasks;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.Interfaces;

public interface IUserService
{
    Task<User> CreateUser(string name, string email);
    Task<ESubscriptionResult> SubscribeUser(Guid userId);
}
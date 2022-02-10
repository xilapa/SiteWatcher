using System;
using System.IO;
using System.Threading.Tasks;
using SiteWatcher.Domain.Entities;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Interfaces;

namespace SiteWatcher.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<User> CreateUser(string name, string email)
    {
        var userToAdd = new User(name, email);
        userToAdd.SecurityStamp = await GenerateUserSecurityStamp(userToAdd);
        return this.userRepository.Add(userToAdd);
    }

    public async Task<ESubscriptionResult> SubscribeUser(Guid userId)
    {
        var user = await this.userRepository.FindAsync(u => u.Id == userId);

        if (user is null)
            return ESubscriptionResult.UserDoesNotExist;            

        if (user.Subscribed)
            return ESubscriptionResult.AlreadySubscribed;

        user.Subscribed = true;

        return ESubscriptionResult.SubscribedSuccessfully;
    }

    private async static Task<string> GenerateUserSecurityStamp(User user)
    {
        byte[] securityBytes;
        using (var stream = new MemoryStream())
        {
            if(user.Id != new Guid())
                await stream.WriteAsync(user.Id.ToString().GetHashedBytes());
            await stream.WriteAsync(user.Name.GetHashedBytes());
            await stream.WriteAsync(user.Email.GetHashedBytes());
            await stream.WriteAsync(DateTime.UtcNow.ToString().GetHashedBytes());
            await stream.WriteAsync(new Random().Next().ToString().GetHashedBytes());

            var _securityBytes = stream.ToArray();
            var _securityBytesHash = BitConverter.GetBytes(stream.GetHashCode());

            securityBytes = new byte[_securityBytes.Length + _securityBytesHash.Length];
            _securityBytes.CopyTo(securityBytes, 0);
            _securityBytesHash.CopyTo(securityBytes, _securityBytes.Length);
        }

        return Convert.ToBase64String(securityBytes);
    }
}
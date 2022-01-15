using System;
using System.IO;
using System.Threading.Tasks;
using AFA.Domain.Entities;
using AFA.Domain.Extensions;
using AFA.Domain.Interfaces;

namespace AFA.Domain.Services;

public class UserService : IUserService
{
    // TODO: trocar por interface de repo
    private readonly AFAContext context;
    public UserService(AFAContext context)
    {
        this.context = context;
    }

    public async Task<User> AddUser(User userToAdd)
    {
        userToAdd.SecurityStamp = await this.GenerateUserSecurityStamp(userToAdd);
        this.context.Users.Add(userToAdd);
        return userToAdd;
    }

    public User SubscribeUser(User user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        if (user.Subscribed)
        {
            // enviar email já esta inscrito
        }
        else
        {
            user.Subscribed = true;
            // enviar email para confirmar a inscrição
        }

        return user;
    }

    private async Task<string> GenerateUserSecurityStamp(User user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        byte[] securityBytes;
        using (var stream = new MemoryStream())
        {
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
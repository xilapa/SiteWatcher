﻿using MediatR;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Users.Commands.DeleteUser;

public class AccountDeletedNotification : INotification
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public ELanguage Language { get; set; }
}
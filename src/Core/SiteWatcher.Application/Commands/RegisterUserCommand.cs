using MediatR;
using SiteWatcher.Application.Metadata;
using SiteWatcher.Application.Validators;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Commands;

public class RegisterUserCommand : Validable<RegisterUserCommand>, IRequest<ApplicationResult<string>>
{
    public RegisterUserCommand() : base(new RegisterUserCommandValidator())
    { }

    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
    public string GoogleId { get; set; }
    public string AuthEmail { get; set; }
}
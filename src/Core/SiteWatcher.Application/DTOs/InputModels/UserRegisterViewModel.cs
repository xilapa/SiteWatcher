using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.DTOs.InputModels;

public class UserRegisterViewModel
{
    public string GoogleId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
}
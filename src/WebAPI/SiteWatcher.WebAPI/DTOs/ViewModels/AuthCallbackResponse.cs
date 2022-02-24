using SiteWatcher.Application.DTOs.InputModels;

namespace SiteWatcher.WebAPI.DTOs.ViewModels;

public class AuthenticationResult 
{
    public AuthenticationResult(EAuthTask task, UserRegisterViewModel registerModel, string token)
    {
        Task = task;
        RegisterModel = registerModel;
        Token = token;
    }

    public EAuthTask Task { get; set; }
    public UserRegisterViewModel RegisterModel { get; set; }
    public string Token { get; set; }
}

public enum EAuthTask
{
    Register = 1,
    Login = 2
}
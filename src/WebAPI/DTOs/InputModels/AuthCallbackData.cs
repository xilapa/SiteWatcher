namespace SiteWatcher.WebAPI.DTOs.InputModels;

// TODO: criar validador para esse input
public class AuthCallBackData
{
    public string? State { get; set; }
    public string? Code { get; set; }
    public string? Scope { get; set; }
}
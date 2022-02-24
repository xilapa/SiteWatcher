namespace SiteWatcher.WebAPI.DTOs.InputModels;

public class AuthCallBackData 
{
    public string State { get; set; }
    public string Code { get; set; }
    public string Scope { get; set; }
}
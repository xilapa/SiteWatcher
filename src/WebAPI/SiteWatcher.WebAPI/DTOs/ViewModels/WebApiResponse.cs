using SiteWatcher.Application.DTOs.Metadata;

namespace SiteWatcher.WebAPI.DTOs.ViewModels;

// TODO: tipar result
public sealed class WebApiResponse
{
    public WebApiResponse()
    {
        Messages = new List<string>();
    }

    public WebApiResponse(object result, params string[] messages) : this()
    {
        if(messages != null) Messages.AddRange(messages);
        Result = result;
    }

    public WebApiResponse(ApplicationResponse appResponse) : this()
    {
        if(appResponse.Success)        
            Messages.Add(appResponse.Message);        
        else        
            Messages.AddRange(appResponse.Errors);
        
        Result = appResponse.Result;
    }

    public List<string> Messages { get; set; }
    public object Result { get; set; }
}
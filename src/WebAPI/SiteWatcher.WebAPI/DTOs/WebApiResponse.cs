using System.Collections.Generic;
using AFA.Application.DTOS.Metadata;

namespace AFA.WebAPI.DTOs;

public sealed class WebApiResponse
{
    public WebApiResponse()
    {
        Messages = new List<string>();
    }

    public WebApiResponse(object result, params string[] messages) : this()
    {
        Messages.AddRange(messages);
        Result = result;
    }

    public WebApiResponse(ApplicationResponse appResponse) : this()
    {
        if(appResponse.Success)
        {
            Messages.Add(appResponse.Message);
        }
        else
        {
            Messages.AddRange(appResponse.Errors);
        }

        Result = appResponse.Result;
    }

    public List<string> Messages { get; set; }
    public object Result { get; set; }
}
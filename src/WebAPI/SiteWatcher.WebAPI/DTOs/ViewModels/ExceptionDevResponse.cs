using System;

namespace SiteWatcher.WebAPI.DTOs.ViewModels;

public class ExceptionDevResponse
{
    public int Code { get; set; }
    public string Type { get; set; }
    public string Message { get; set; }
    public string TraceId { get; set; }
    public string StackTrace { get; set; }
    public InnerExceptionDevResponse InnerException { get; set; }


    public static ExceptionDevResponse From(Exception exception, string traceId)
    {
        if(exception is null)
            return null;

        return new ExceptionDevResponse
                    {
                        Code = exception.HResult,
                        Type = exception.GetType().Name,
                        Message = exception.Message,
                        TraceId = traceId,
                        StackTrace = exception.StackTrace,
                        InnerException = InnerExceptionDevResponse.From(exception.InnerException)
                    };
    }
}

public class InnerExceptionDevResponse
{
    public int Code { get; set; }
    public string Type { get; set; }
    public string Message { get; set; }
    public InnerExceptionDevResponse InnerException { get; set; }

    public static InnerExceptionDevResponse From(Exception exception)
    {
        if(exception is null)
            return null;

        return new InnerExceptionDevResponse
                    {
                        Code = exception.HResult,
                        Type = exception.GetType().Name,
                        Message = exception.Message,
                        InnerException = From(exception.InnerException)
                    };
    }
}
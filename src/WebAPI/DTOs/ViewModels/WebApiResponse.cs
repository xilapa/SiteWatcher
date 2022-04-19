namespace SiteWatcher.WebAPI.DTOs.ViewModels;

public sealed class WebApiResponse<T> where T : class?
{
    public WebApiResponse()
    {
        Messages = new List<string>();
    }

    public WebApiResponse(T result, params string[]? msgs) : this()
    {
        if(msgs != null) Messages.AddRange(msgs);
        Result = result;
    }

    public WebApiResponse(T result, IEnumerable<string>? msgs) : this()
    {
        if(msgs != null) Messages.AddRange(msgs);
        Result = result;
    }

    public WebApiResponse<T> AddMessages(params string[]? msgs)
    {
       if(msgs != null) Messages.AddRange(msgs);
       return this;
    }

    public WebApiResponse<T> AddMessages(IEnumerable<string>? msgs)
    {
       if(msgs != null) Messages.AddRange(msgs);
       return this;
    }

    public WebApiResponse<T> SetResult(T result)
    {
        Result = result;
        return this;
    }

    public List<string> Messages { get; set; }
    public T? Result { get; set; }
}
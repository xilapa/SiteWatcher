namespace SiteWatcher.Application.Metadata;

public class ApplicationResult<T>
{
    public ApplicationResult() =>    
        _errors = new List<string>();
    
    public ApplicationResult(T result) : this() =>    
        Value = result;
    

    public ApplicationResult<T> AddError(string error)
    {
        _errors.Add(error);
        return this;
    }

    public ApplicationResult<T> SetValue(T value)
    {
        Value = value;
        return this;
    }

    private readonly List<string> _errors;
    public IReadOnlyCollection<string> Errors => _errors;
    public bool Success => !Errors.Any();
    public T Value { get; set; }
}
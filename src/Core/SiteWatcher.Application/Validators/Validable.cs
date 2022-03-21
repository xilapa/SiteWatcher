using FluentValidation;
using SiteWatcher.Domain.Interfaces;

namespace SiteWatcher.Application.Validators;

public abstract class Validable<T> : IValidable where T : class
{
    public Validable(AbstractValidator<T> validator) 
    {
        _errors = new List<string>();
        _validator = validator;
    }

    private readonly AbstractValidator<T> _validator;
    private readonly List<string> _errors;
    public bool IsValid { get => _errors.Any(); }
    public bool IsInvalid { get => !_errors.Any(); }

    public IEnumerable<string> Errors  { get => _errors.ToArray(); }

    protected void AddErrors(IEnumerable<string> errors) =>    
        _errors.AddRange(errors);    

    public IEnumerable<string> Validate()
    {
        var result = _validator.Validate(this as T);
        AddErrors(result.Errors.Select(e => e.ErrorMessage));
        return Errors;
    }
}
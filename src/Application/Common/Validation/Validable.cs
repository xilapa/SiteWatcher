using FluentValidation;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Common.Validation;

public abstract class Validable<T> : IValidable where T : class
{
    protected Validable(AbstractValidator<T> validator)
    {
        _errors = new List<string>();
        _validator = validator;
    }

    private readonly AbstractValidator<T> _validator;
    private readonly List<string> _errors;
    public bool IsValid => _errors.Count > 0;
    public bool IsInvalid => _errors.Count == 0;

    public IEnumerable<string> Errors  => _errors.ToArray();

    private void AddErrors(IEnumerable<string> errors) =>
        _errors.AddRange(errors);

    public IEnumerable<string> Validate()
    {
        var result = _validator.Validate((this as T)!);
        AddErrors(result.Errors.Select(e => e.ErrorMessage));
        return Errors;
    }
}
using FluentValidation;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Common.Validation;

public abstract class Validable<T> : IValidable where T : class
{
    public async Task<string[]> ValidateAsyncWith(IValidator validator)
    {
        if (validator is not AbstractValidator<T> abstractValidator)
            throw new ArgumentException($"The {nameof(validator)} is not an AbstractValidator");

        if (this is not T instance)
            throw new Exception("Something wrong is not right");

        var result = await abstractValidator.ValidateAsync(instance);
        if (result.IsValid)
            return Array.Empty<string>();

        var errors = result.Errors.Select(err => err.ErrorMessage)
            .ToArray();
        return errors;
    }
}
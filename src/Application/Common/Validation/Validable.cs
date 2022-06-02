using FluentValidation;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Common.Validation;

public abstract class Validable<T> : IValidable where T : class
{
    public async Task<string[]> ValidateAsyncWith(dynamic validator)
    {
        if (validator is not AbstractValidator<T> _validator)
            return Array.Empty<string>();

        var result = await _validator.ValidateAsync((this as T)!);
        if (result.IsValid)
            return Array.Empty<string>();

        var errors = result.Errors.Select(err => err.ErrorMessage)
            .ToArray();
        return errors;
    }
}
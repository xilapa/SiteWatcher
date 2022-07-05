using FluentValidation;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Common.Validation;

public abstract class Validable<T> : IValidable where T : class
{
    public async Task<string[]> ValidateAsyncWith(IValidator validator)
    {
        var result = await (validator as AbstractValidator<T>)!.ValidateAsync((this as T)!);
        if (result.IsValid)
            return Array.Empty<string>();

        var errors = result.Errors.Select(err => err.ErrorMessage)
            .ToArray();
        return errors;
    }
}
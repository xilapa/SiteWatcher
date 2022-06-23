using FluentValidation;

namespace SiteWatcher.Application.Interfaces;

public interface IValidable
{
    Task<string[]> ValidateAsyncWith(IValidator validator);
}
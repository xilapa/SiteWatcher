using FluentValidation;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Domain.Common.Errors;

namespace SiteWatcher.Application.Common.Command;

public abstract class BaseHandler<T,R> : IApplicationHandler where R : IResult, new()
{
    private readonly IValidator<T> _validator;

    protected BaseHandler(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async Task<R> Handle(T command, CancellationToken ct)
    {
        var res = _validator.Validate(command);
        if (res.IsValid)
            return await HandleCommand(command, ct);

        var errors = res.Errors.Select(err => err.ErrorMessage).ToArray();
        var errorResult = new R();
        errorResult.SetError(Error.Validation(errors));
        return errorResult;
    }

    protected abstract Task<R> HandleCommand(T command, CancellationToken ct);
}

public interface IApplicationHandler{}
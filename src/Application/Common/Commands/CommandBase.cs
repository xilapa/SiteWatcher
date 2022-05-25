using FluentValidation;
using MediatR;
using SiteWatcher.Application.Common.Validation;

namespace SiteWatcher.Application.Common.Commands;

public class CommandBase<TCommand, TResponse> : Validable<TCommand>, IRequest<CommandResult<TResponse>> where
    TCommand : class
{
    public CommandBase(AbstractValidator<TCommand> validator) : base(validator)
    { }
}
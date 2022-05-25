using MediatR;

namespace SiteWatcher.Application.Common.Commands;

public interface ICommandHandlerBase<TCommand, TResponse> : IRequestHandler<TCommand, ICommandResult<TResponse>>
where TCommand : IRequest<ICommandResult<TResponse>>
{
}
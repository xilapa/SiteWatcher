using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Common.Commands;
using EmptyResult = SiteWatcher.Application.Common.Commands.EmptyResult;

namespace SiteWatcher.WebAPI.Extensions;

public static class CommandResultExtensions
{
    private const string ResultIsNotEmpty = "The result has a value, use instead Handle<T>";

    /// <summary>
    /// Handle command results with value.
    /// </summary>
    /// <param name="commandResult">Command result to be handled</param>
    /// <param name="valueResultHandler">Result handler, the default value is OkObjectResult</param>
    /// <typeparam name="T">Type of the CommandValueResult</typeparam>
    /// <returns>Action result</returns>
    public static IActionResult Handle<T>(this CommandResult commandResult, Func<T, IActionResult>? valueResultHandler = null) =>
        commandResult switch
        {
            EmptyResult => new OkResult(),
            ErrorResult errorResult => new BadRequestObjectResult(errorResult.Errors),
            ValueResult<T> valueResult => valueResultHandler is null ?
                new OkObjectResult(valueResult.Value) : valueResultHandler(valueResult.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(commandResult), commandResult, null)
        };

    /// <summary>
    /// Handle empty command results.
    /// </summary>
    /// <param name="commandResult"></param>
    /// <returns>Action result</returns>
    /// <exception cref="InvalidOperationException">The result has a value</exception>
    public static IActionResult Handle(this CommandResult commandResult) =>
        commandResult switch
        {
            EmptyResult => new OkResult(),
            ErrorResult errorResult => new BadRequestObjectResult(errorResult.Errors),
            ValueResult<object> => throw new InvalidOperationException(ResultIsNotEmpty),
            _ => throw new ArgumentOutOfRangeException(nameof(commandResult), commandResult, null)
        };
}
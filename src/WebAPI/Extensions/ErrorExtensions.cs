using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Domain.Common.Errors;

namespace SiteWatcher.WebAPI.Extensions;

public static class ErrorExtensions
{
    public static IActionResult ToActionResult(this Error error)
    {
        return error switch
        {
            { Type: ErrorType.Validation } => new BadRequestObjectResult(error.Messages),
            _ => throw new ArgumentOutOfRangeException(nameof(error))
        };
    }
}
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AFA.WebAPI.Filters;

public class ActionFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if(!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                                .SelectMany(m => m.Value.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToArray();

            context.Result = new BadRequestObjectResult(errors);
        }
    }
}
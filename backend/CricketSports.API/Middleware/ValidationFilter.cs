using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CricketSports.API.Middleware;

public sealed class ValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var value in context.ActionArguments.Values.Where(value => value is not null))
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(value!.GetType());
            if (services.GetService(validatorType) is not IValidator validator) continue;
            var result = await validator.ValidateAsync(new ValidationContext<object>(value));
            foreach (var group in result.Errors.GroupBy(error => error.PropertyName)) errors[group.Key] = group.Select(error => error.ErrorMessage).ToArray();
        }
        if (errors.Count > 0) { context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)); return; }
        await next();
    }
}

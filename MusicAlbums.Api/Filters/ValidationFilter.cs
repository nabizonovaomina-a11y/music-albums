using FluentValidation;

namespace MusicAlbums.Api.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();
        var arg = ctx.Arguments.OfType<T>().FirstOrDefault();
        if (validator is null || arg is null)
            return await next(ctx);

        var result = await validator.ValidateAsync(arg);
        if (result.IsValid)
            return await next(ctx);

        var errors = result.Errors
            .GroupBy(e => char.ToLowerInvariant(e.PropertyName[0]) + e.PropertyName[1..])
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Middleware;

public sealed class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;

    public ProblemDetailsMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private static Task WriteProblemAsync(HttpContext ctx, Exception ex)
    {
        var (status, title) = MapStatus(ex);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ex.Message,
            Instance = ctx.Request.Path
        };

        if (ex is ArgumentException argEx && !string.IsNullOrWhiteSpace(argEx.ParamName))
        {
            problem.Extensions["param"] = argEx.ParamName;
        }

        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteAsJsonAsync(problem);
    }

    private static (int status, string title) MapStatus(Exception ex) => ex switch
    {
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
        ArgumentNullException => (StatusCodes.Status400BadRequest, "Bad Request"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };
}

public static class ProblemDetailsMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalProblemDetails(this IApplicationBuilder app)
        => app.UseMiddleware<ProblemDetailsMiddleware>();
}

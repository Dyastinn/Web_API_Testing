using System.Text.Json;

namespace SimpleApi.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (404, exception.Message),
            ArgumentException => (400, exception.Message),
            _ => (500, exception.Message)
        };

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new { message, statusCode });
    }
}

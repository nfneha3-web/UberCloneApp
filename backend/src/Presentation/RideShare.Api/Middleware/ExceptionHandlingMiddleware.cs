using System.Net;
using System.Text.Json;
using RideShare.Application.Common.Exceptions;
using RideShare.Domain.Exceptions;
using ValidationException = RideShare.Application.Common.Exceptions.ValidationException;

namespace RideShare.Api.Middleware;

/// <summary>Translates Application/Domain exceptions into consistent ProblemDetails HTTP responses.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (HttpStatusCode.BadRequest, "Validation failed"),
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "Forbidden"),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            DomainException => (HttpStatusCode.BadRequest, "Business rule violation"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogInformation("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        object body = exception is ValidationException validationException
            ? new { title, status = (int)statusCode, errors = validationException.Errors }
            : new { title, status = (int)statusCode, detail = exception.Message };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}

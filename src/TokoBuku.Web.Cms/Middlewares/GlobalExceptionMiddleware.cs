using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TokoBuku.Domain.Exceptions;

namespace TokoBuku.Web.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        context.Response.Headers["X-Correlation-ID"] = traceId;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, string traceId)
    {
        var user = context.User.Identity?.Name ?? "Anonymous";
        var method = context.Request.Method;
        var path = context.Request.Path + context.Request.QueryString;

        // Structured Logging for Developers / SRE
        _logger.LogError(
            ex,
            "[SYSTEM INCIDENT] TraceId: {TraceId} | User: {User} | Route: {Method} {Path} | Error: {Message}",
            traceId,
            user,
            method,
            path,
            ex.Message
        );

        var isJsonRequest = context.Request.Headers.Accept.Any(h => h != null && h.Contains("application/json"))
                            || context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                            || context.Request.Path.StartsWithSegments("/api")
                            || context.Request.Path.StartsWithSegments("/Pos/Checkout");

        if (isJsonRequest)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                DomainException => (int)HttpStatusCode.BadRequest,
                FluentValidation.ValidationException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var errorResponse = new
            {
                success = false,
                status = context.Response.StatusCode,
                title = "Terjadi Kendala Pemrosesan",
                message = ex is DomainException or FluentValidation.ValidationException
                    ? ex.Message
                    : "Sistem mengalami kendala pemrosesan. Hubungi tim teknis dengan menyebutkan Kode Referensi berikut.",
                traceId,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
        else
        {
            // Store exception in Items so HomeController.Error can render developer debug details
            context.Items["LastError"] = ex;
            context.Items["LastErrorTraceId"] = traceId;
            context.Response.Redirect($"/Home/Error?traceId={traceId}");
        }
    }
}

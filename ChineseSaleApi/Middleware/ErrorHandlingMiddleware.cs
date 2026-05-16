using System;
using System.Net;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            string message;

            switch (exception)
            {
                case ArgumentNullException _:
                case ArgumentException _:
                    status = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    _logger.LogWarning(exception, "Client error: {Message}", exception.Message);
                    break;
                case KeyNotFoundException _:
                    status = HttpStatusCode.NotFound;
                    message = exception.Message;
                    _logger.LogWarning(exception, "Not found: {Message}", exception.Message);
                    break;
                case UnauthorizedAccessException _:
                    status = HttpStatusCode.Unauthorized;
                    message = exception.Message ?? "Unauthorized";
                    _logger.LogWarning(exception, "Unauthorized access: {Message}", exception.Message);
                    break;
                case SecurityException _:
                    status = HttpStatusCode.Forbidden;
                    message = exception.Message ?? "Forbidden";
                    _logger.LogWarning(exception, "Forbidden: {Message}", exception.Message);
                    break;
                default:
                    status = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred.";
                    _logger.LogError(exception, "Unhandled exception occurred.");
                    break;
            }

            var result = JsonSerializer.Serialize(new { error = message, status = (int)status });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            return context.Response.WriteAsync(result);
        }
    }
}

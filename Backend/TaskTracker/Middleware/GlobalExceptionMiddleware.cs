using System.Text.Json;
using TaskTracker.Services.Exceptions;

namespace TaskTracker.Middleware
{
    public sealed class GlobalExceptionMiddleware
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
            try
            {
                await _next(context);
            }
            catch (ServiceExceptioncs ex)
            {
                _logger.LogWarning("Request failed with status {StatusCode} and code {ErrorCode}.", ex.StatusCode, ex.ErrorCode);
                await WriteErrorResponseAsync(context,StatusCodes.Status500InternalServerError,"internal_server_error", "An unexpected error occured. Please try again later");
            }
        }

        private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, string errorCode, string message)
        {
            if(context.Response.HasStarted)
            {
                throw new InvalidOperationException("The resoponce has alredy started");
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                statusCode,
                errorCode,
                message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse), context.RequestAborted);
        }

    }
}

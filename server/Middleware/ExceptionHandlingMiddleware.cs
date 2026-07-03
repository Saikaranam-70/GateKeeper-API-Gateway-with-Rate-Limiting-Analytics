using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using GateKeeper.Exceptions;

namespace GateKeeper.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request processing.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var errorName = "Internal Server Error";
            var message = "An unexpected error occurred. Please try again later.";

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    errorName = "Not Found";
                    message = notFoundEx.Message;
                    break;
                case BadRequestException badRequestEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorName = "Bad Request";
                    message = badRequestEx.Message;
                    break;
                default:
                    message = exception.Message; // Keep message in development/demo context
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                error = errorName,
                message = message,
                statusCode = (int)statusCode
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}

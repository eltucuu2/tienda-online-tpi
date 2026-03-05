using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Pasa al siguiente middleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message); // Log interno

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex switch
                {
                    BadRequestException => (int)HttpStatusCode.BadRequest,             // 400
                    UnauthorizedException => (int)HttpStatusCode.Unauthorized,         // 401
                    NotFoundException => (int)HttpStatusCode.NotFound,                 // 404
                    EntityNotFoundException => (int)HttpStatusCode.NotFound,           // 404
                    ConflictException => (int)HttpStatusCode.Conflict,                 // 409
                    DuplicatedEntityException => (int)HttpStatusCode.Conflict,         // 409
                    NoContentException => (int)HttpStatusCode.NoContent,               // 204
                    InternalServerErrorException => (int)HttpStatusCode.InternalServerError, // 500
                    _ => (int)HttpStatusCode.InternalServerError
                };

                object response;
                if (_env.IsDevelopment())
                {
                    response = (new {message = ex.Message}); // Aqui va el new ApiError()
                }
                else
                {
                    response = (new { message = "Ha ocurrido un error inesperado." });
                }

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }

        
    }
}

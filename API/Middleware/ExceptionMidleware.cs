

using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware
{
    public class ExceptionMidleware
    {
        private readonly RequestDelegate _next ;
        private readonly ILogger<ExceptionMidleware> _logger;
        private readonly IHostEnvironment _env ;
        public ExceptionMidleware(RequestDelegate next, ILogger<ExceptionMidleware> logger, IHostEnvironment env)
        {
            this._env = env;
            this._logger = logger;
            this._next = next;
            
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var responce = _env.IsDevelopment()
                                ?new APIException(context.Response.StatusCode,ex.Message,ex.StackTrace?.ToString())
                                :new APIException(context.Response.StatusCode,ex.Message,"Internal Server Error");

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase}                ;

                var json = JsonSerializer.Serialize(responce,options);

                await context.Response.WriteAsync(json);
            }
            
        }
    }
}
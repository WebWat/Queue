using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebApplication1
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CheckUser
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CheckUser> _logger;

        public CheckUser(RequestDelegate next, ILogger<CheckUser> logger)
        {
            _next = next;
            _logger = logger;
        }

        public Task Invoke(HttpContext httpContext)
        {
            _logger.LogInformation($"New request {httpContext.Connection.Id}");

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CheckUserExtensions
    {
        public static IApplicationBuilder UseCheckUser(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CheckUser>();
        }
    }
}

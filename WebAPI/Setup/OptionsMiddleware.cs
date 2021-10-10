using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Setup
{
    // https://stackoverflow.com/questions/42199757/enable-options-header-for-cors-on-net-core-web-api
    public class OptionsMiddleware
    {
        private readonly RequestDelegate _next;

        public OptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            string origin = context.Request.Headers["Origin"];
            context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, Host, Content-Type, Accept, Authorization, Cookie, X-Requested-With");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE");
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                return Task.CompletedTask;
            }
            else return _next.Invoke(context);
        }
    }
}

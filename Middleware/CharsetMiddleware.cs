using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BeatSlayerServer.Middleware
{
    public class CharsetMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string encoding;

        public CharsetMiddleware(RequestDelegate next, string encoding)
        {
            this.next = next;
            this.encoding = encoding;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.ContentType = "text/html; charset=" + encoding;
            await next.Invoke(context);
        }
    }
}

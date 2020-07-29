using BeatSlayerServer.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BeatSlayerServer.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCharset(this IApplicationBuilder app, string encoding)
        {
            return app.UseMiddleware<CharsetMiddleware>(encoding);
        }
    }
}

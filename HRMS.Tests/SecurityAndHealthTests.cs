using System.Threading.Tasks;
using HRMS.API.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HRMS.Tests
{
    public class SecurityAndHealthTests
    {
        [Fact]
        public async Task SecurityHeadersMiddleware_AppliesAllHardenedSecurityHeaders()
        {
            var context = new DefaultHttpContext();
            var middleware = new SecurityHeadersMiddleware(innerContext =>
            {
                innerContext.Response.StatusCode = 200;
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context);

            Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
            Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
            Assert.Equal("1; mode=block", context.Response.Headers["X-XSS-Protection"]);
            Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
            Assert.Contains("frame-ancestors 'none'", context.Response.Headers["Content-Security-Policy"].ToString());
            Assert.Contains("camera=()", context.Response.Headers["Permissions-Policy"].ToString());
        }
    }
}

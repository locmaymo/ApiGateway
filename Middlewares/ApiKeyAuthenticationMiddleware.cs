using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ApiGateway.Services;
using System.Net;

namespace ApiGateway.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYHEADER = "X-Api-Key";

        public ApiKeyAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/gateway", StringComparison.OrdinalIgnoreCase))
            {
                // If not gateway endpoint, skip
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(APIKEYHEADER, out var extractedApiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("API Key is missing.");
                return;
            }

            // Resolve the scoped ApiKeyService from the request's service provider
            var apiKeyService = context.RequestServices.GetRequiredService<ApiKeyService>();

            var user = await apiKeyService.GetUserByApiKeyAsync(extractedApiKey);

            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid API Key.");
                return;
            }

            // Optionally, you can set the user in the HttpContext for further use
            context.Items["User"] = user;

            // Proceed to the next middleware/component
            await _next(context);
        }
    }
}
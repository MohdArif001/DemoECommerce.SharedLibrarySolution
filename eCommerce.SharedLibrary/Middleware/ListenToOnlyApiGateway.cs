using Microsoft.AspNetCore.Http;

namespace eCommerce.SharedLibrary.Middleware
{
    public class ListenToOnlyApiGateway(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // EXTRACT SPECIFIC HEADER FROM THE REQUEST
            var signedHeader = context.Request.Headers["Api-Gateway"];

            // NULL MEANS , THE REQUEST IS NOT CMING FROM THE API GATEWAY // 503 SERVICE UNAVAILABLE
            if (signedHeader.FirstOrDefault() is null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("sorry , service unavailable");
                return;
            }
            else
            {
                await next(context);
            }
        }
    }
}

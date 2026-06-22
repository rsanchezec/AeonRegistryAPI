namespace AeonRegistryAPI.Middleware
{
    public class BlockIdentityEndpoints
    {

        private readonly RequestDelegate _next;

        private static readonly string[] BlockedPaths =
        [
             "/api/auth/register",
             "/api/auth/forgotpassword",
             "/api/auth/resetpassword",
             "/api/auth/manage",
             "/api/auth/manage/info"
        ];

        public BlockIdentityEndpoints(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();

            if (path != null && BlockedPaths.Contains(path))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Not Found");
                return;
            }

            await _next(context);
        }
    }
}

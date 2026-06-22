using Microsoft.AspNetCore.Http.HttpResults;

namespace AeonRegistryAPI.Endpoints.Home
{
    public static class HomeEndpoints
    {
        public static IEndpointRouteBuilder MapHomeEndpoints(this IEndpointRouteBuilder route)
        {
            var homeGroup = route.MapGroup("/api/Home")
              .WithTags("Home");

            ///api/home/welcome
            homeGroup.MapGet("/welcome", GetWelcomeMessage)
                .WithName("GetWelcomeMessage")
                .WithSummary("Welcome Message")
                .WithDescription("Displays a welcome message");

            return route;
        }

        // Handlers
        private static async Task<Ok<WelcomeResponse>> GetWelcomeMessage(CancellationToken ct)
        {
            var welcomeMessage = new WelcomeResponse
            {
                Message = "Welcome to the Aeon Registry API!",
                Version = "1.0.0",
                TimeOnly = DateTime.Now.ToString("T")
            };

            return TypedResults.Ok(welcomeMessage);
        }

    }
}

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace AeonRegistryAPI.Endpoints.CustomIndentityEndpoints.Models
{
    public class UserProfileResponse
    {

        public string? Id { get; set; }

        public string? Email { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? FullName { get; set; }
    }
}
using AeonRegistryAPI.Endpoints.CustomIndentityEndpoints.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace AeonRegistryAPI.Endpoints.CustomIndentityEndpoints
{
    public static class CustomIdentityEndpoints
    {

        public static IEndpointRouteBuilder MapCustomIdentityEndpoints(this IEndpointRouteBuilder route)
        {
            //step one make a group
            var group = route.MapGroup("/api/auth")
               .WithTags("Admin");


            group.MapPost("/register-admin", RegisterUser)
                .WithName("RegisterAdmin")
                .WithSummary("Register a User")
                .WithDescription("Registers a user must have admin role")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest); ;
            //.RequireAuthorization("AdminPolicy");

            group.MapPost("/reset-password", ResetPassword)
                .WithName("ResetPassword")
                .WithDescription("Custom Reset Password for a user")
                .WithSummary("Custom Reset Password")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/forgot-password", ForgotPassword)
                .WithName("ForgotPassword")
                .WithDescription("Custom Forgot password flow")
                .WithSummary("Custom Forgot Password")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/manage/profile", GetProfileInfo)
                .WithName("GetPRofileInfo")
                .WithDescription("Get current user profile info")
                .WithSummary("Get the current users profile")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();

            group.MapPut("/manage/profile", UpdateProfileInfo)
                .WithName("UpdateProfile")
                .WithDescription("Updates the current user profile")
                .WithSummary("Allos the current yser to update their profile.")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();

            group.MapGet("/manage/users", ListAllUsers)
                .WithName("ListUsers")
                .WithDescription("List All Users")
                .WithSummary("List all registered users")
                .RequireAuthorization()
                .Produces<IEnumerable<UserProfileResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);



            return route;
        }

        private static async Task<IResult> RegisterUser(
            RegisterUserRequest dto,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IConfiguration config
            )
        {

            //see if the user email sent already exists
            if (await userManager.FindByEmailAsync(dto.Email) is not null)
            {
                return Results.BadRequest(new { Error = $"User with email {dto.Email} already exists" });
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var tempPassword = "Admin123!"; //Meet the password requirements

            var created = await userManager.CreateAsync(user, tempPassword);

            if (!created.Succeeded)
            {
                return Results.BadRequest(new { Error = created.Errors });
            }

            if (await roleManager.RoleExistsAsync("Researcher"))
            {
                await userManager.AddToRoleAsync(user, "Researcher");
            }

            //generate password reset token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            //send email to user to change password

            var baseURL = config["BaseURL"] ?? "https://localhost:7028";

            await emailSender.SendEmailAsync(
                dto.Email,
                "Welcome to the Aeon Registry",
                $"""
                Your account has been created. Please change your password by visiting: {baseURL}/Setpassword.html
                
                {baseURL}/Setpassword.html?email={dto.Email}&resetCode={encodedToken}
                
                """
                );

            return Results.Ok(new { Message = $"User {user.Email} created. Password reset link sent" });
        }

        private static async Task<IResult> ResetPassword(
            ResetPasswordRequest request,
            UserManager<ApplicationUser> userManager)
        {

            if (string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.ResetCode) ||
                string.IsNullOrEmpty(request.NewPassword))
            {
                return Results.BadRequest(new { Message = "All fields are required" });
            }

            //find the user
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Results.BadRequest(new { Message = "User not found. " });
            }

            try
            {
                var decodedToken = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(request.ResetCode));

                var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

                if (result.Succeeded)
                {
                    return Results.Ok(new { Message = "Password reset successful." });
                }

                return Results.BadRequest(new { Message = "Error" });
            }
            catch (FormatException)
            {
                return Results.BadRequest(new { Message = "Invalid Token" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = $"Error: {ex.Message}" });
            }

        }

        private static async Task<IResult> ForgotPassword(
            ForgotPasswordRequest request,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IConfiguration config
            )
        {

            if (string.IsNullOrEmpty(request.Email))
            {
                return Results.BadRequest(new { Message = "Email Address is required" });
            }

            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Results.Ok(new { Message = "If the user exists a forgot password link will be sent." });
            }

            //generate a reset token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));


            var baseURL = config["BaseURL"] ?? "https://localhost:7028";
            var resetLink = $"{baseURL}/reset-password.html?email={user.Email}&resetCode={encodedToken}";

            await emailSender.SendEmailAsync(
                request.Email,
                "Reset your Password",
                $"""
                To Reset your password, use the link:
                
                {resetLink}
                
                """
                );

            return Results.Ok(new { Message = "If the user exists a forgot password link will be sent." });
        }

        private static async Task<IResult> GetProfileInfo(
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager
            )
        {

            var user = await userManager.GetUserAsync(principal);

            if (user is null)
            {
                return Results.NotFound();
            }

            var response = new UserProfileResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FullName = user.FullName
            };

            return Results.Ok(response);
        }

        private static async Task<IResult> UpdateProfileInfo(
            UpdateUserProfileRequest request,
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager
            )
        {
            //validate inputs
            if (String.IsNullOrEmpty(request.FirstName) || String.IsNullOrEmpty(request.LastName))
            {
                return Results.BadRequest(new { Message = "First and Last name are required." });
            }

            var user = await userManager.GetUserAsync(principal);

            if (user is null)
            {
                return Results.NotFound();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { Message = $"Update Failed: {result.Errors}" });
            }

            return Results.Ok(new { Message = "Profile update successfully" });
        }

        private static async Task<IResult> ListAllUsers(
            UserManager<ApplicationUser> userManager
            )
        {
            var users = userManager.Users
              .Select(u => new UserProfileResponse
              {
                  Id = u.Id,
                  FirstName = u.FirstName,
                  LastName = u.LastName,
                  FullName = u.FullName,
                  Email = u.Email
              })
              .ToList();

            return Results.Ok(users);
        }
    }
}
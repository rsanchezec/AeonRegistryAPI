---
name: aspnet-api-identity-starter
description: Scaffold and configure a reusable .NET 10 ASP.NET Core Web API foundation with Entity Framework Core, ASP.NET Identity, token-based authentication, roles, Swagger Bearer authorization, endpoint modules, services, middleware, configuration, migrations, and validation. Use when starting or upgrading a .NET API project that needs users, login, protected endpoints, roles, and a clean backend architecture.
---

# ASP.NET API Identity Starter

Use this skill when the user wants to create or complete the base architecture for an ASP.NET Core API with authentication, authorization, database persistence, Swagger, endpoint organization, services, middleware, and token-based login.

This skill can be used in Codex as a real skill, or copied into Claude as project instructions / a reusable prompt.

## Primary Goal

Build a clean starter foundation for APIs that need:

- .NET 10 ASP.NET Core Minimal API or Web API backend.
- Entity Framework Core database access.
- ASP.NET Identity users and roles.
- Login with access tokens.
- A ready-to-use authentication endpoint module.
- Swagger configured for Bearer tokens.
- Organized project folders.
- Endpoint modules instead of putting everything in `Program.cs`.
- Services for reusable logic.
- Middleware for cross-cutting request behavior.
- Seed data for roles and optional admin user.
- Build/test verification.

The finished starter must expose these working API endpoints unless the user explicitly asks for a smaller setup:

```text
POST /api/auth/login
POST /api/auth/register-admin
POST /api/auth/reset-password
POST /api/auth/forgot-password
GET  /api/auth/manage/profile
PUT  /api/auth/manage/profile
GET  /api/auth/manage/users
GET  /api/home/welcome
```

The endpoint names, route grouping, DTOs, services, authorization requirements, and Swagger documentation must be ready for immediate local testing.

## Before Making Changes

1. Inspect the project structure.
2. Identify the .NET target version.
3. Identify whether the project uses Minimal APIs, controllers, or both.
4. Identify the database provider: PostgreSQL, SQL Server, SQLite, or unknown.
5. Inspect `Program.cs`, `.csproj`, `appsettings.json`, existing `Data`, `Models`, `Endpoints`, `Services`, `Extensions`, and `Middleware` folders.
6. Preserve existing code and conventions.
7. Do not overwrite user changes.
8. If Identity or EF Core already exists, extend the current setup instead of recreating it from scratch.

## .NET Version Rule

For a brand-new project, target .NET 10:

```xml
<TargetFramework>net10.0</TargetFramework>
```

For an existing project:

- If it already targets `net10.0`, keep .NET 10 and use compatible package versions.
- If it targets another framework, do not change the target framework unless the user asks for an upgrade.
- If the user asks to upgrade to this starter, recommend .NET 10 but explain any package/version changes before editing.

When installing packages for a .NET 10 project, prefer package versions compatible with .NET 10 and keep versions aligned across Microsoft packages.

## Preferred Folder Structure

Use this structure unless the existing project already has a clear convention:

```text
ProjectName/
  Data/
    ApplicationDbContext.cs
    DataUtility.cs
    SeedData.cs
  Models/
    ApplicationUser.cs
    DomainEntity.cs
  Models/Response/
    UserProfileResponse.cs
    ApiResponse.cs
  Endpoints/
    Auth/
      AuthEndpoints.cs
      Models/
        RegisterUserRequest.cs
        ForgotPasswordRequest.cs
        ResetPasswordRequest.cs
        UpdateUserProfileRequest.cs
        UserProfileResponse.cs
    Home/
      HomeEndpoints.cs
    ExampleResource/
      ExampleResourceEndpoints.cs
      Models/
        ExampleCreateRequest.cs
        ExampleUpdateRequest.cs
        ExampleResponse.cs
  Services/
    ConsoleEmailService.cs
    Interfaces/
      IEmailService.cs
  Extensions/
    OpenApiSwaggerExtensions.cs
    ServiceCollectionExtensions.cs
    EndpointRouteBuilderExtensions.cs
  Middleware/
    BlockIdentityEndpoints.cs
    ExceptionHandlingMiddleware.cs
  Migrations/
  wwwroot/
    images/
    docs/
  Program.cs
  appsettings.json
```

## Required Packages

Install only what is missing.

For PostgreSQL:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

For SQL Server, use:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

Do not install both PostgreSQL and SQL Server providers unless the project intentionally supports both.

## Core Classes

### `ApplicationUser`

Create this in `Models/ApplicationUser.cs`.

```csharp
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjectName.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
```

If the project already has an `ApplicationUser`, preserve its fields and add only what is needed.

### `ApplicationDbContext`

Create this in `Data/ApplicationDbContext.cs`.

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectName.Models;

namespace ProjectName.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
}
```

Add domain `DbSet<T>` properties as business entities are introduced:

```csharp
public DbSet<Product> Products => Set<Product>();
```

## `Program.cs` Configuration

Keep `Program.cs` small and readable.

Expected order:

1. Create builder.
2. Register Swagger/OpenAPI.
3. Read connection string.
4. Register `ApplicationDbContext`.
5. Register Identity.
6. Register authorization policies.
7. Register app services.
8. Build app.
9. Enable Swagger in development.
10. Use HTTPS/static files.
11. Use authentication.
12. Use authorization.
13. Use custom middleware.
14. Map endpoints.
15. Run app.

Pattern:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectName.Data;
using ProjectName.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomSwagger();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

builder.Services.AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapIdentityApi<ApplicationUser>();
app.MapCustomIdentityEndpoints();
app.MapHomeEndpoints();

app.Run();
```

Adjust `UseNpgsql` to `UseSqlServer` or another provider when appropriate.

## Swagger Bearer Configuration

Create `Extensions/OpenApiSwaggerExtensions.cs`.

```csharp
using Microsoft.OpenApi;

namespace ProjectName.Extensions;

public static class OpenApiSwaggerExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Project API",
                Version = "v1",
                Description = "API documentation."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter a valid Bearer token."
            });

            c.AddSecurityRequirement(openApiDocument => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", openApiDocument, null),
                    new List<string>()
                }
            });
        });

        return services;
    }
}
```

## Endpoint Module Pattern

Do not leave all endpoints in `Program.cs`.

Create endpoint modules like this:

```csharp
namespace ProjectName.Endpoints.Home;

public static class HomeEndpoints
{
    public static IEndpointRouteBuilder MapHomeEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/home")
            .WithTags("Home");

        group.MapGet("/welcome", GetWelcomeMessage)
            .WithName("WelcomeMessage")
            .Produces(StatusCodes.Status200OK);

        return route;
    }

    private static IResult GetWelcomeMessage()
    {
        return Results.Ok(new
        {
            Message = "Welcome to the API",
            Version = "1.0.0",
            Time = DateTime.UtcNow
        });
    }
}
```

Use this convention:

- Public endpoints: no authorization.
- User endpoints: `.RequireAuthorization()`.
- Admin endpoints: `.RequireAuthorization("AdminOnly")`.
- Keep handler methods private inside the endpoint class unless reused.

## Custom Identity Endpoints

Use custom auth endpoints when the default Identity endpoints need a different shape.

Required starter auth endpoints:

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | Public | Default Identity login endpoint. Returns token/cookie response depending on Identity configuration. |
| `POST` | `/api/auth/register-admin` | Public at first, optionally admin-only later | Creates a user with a temporary valid password and sends/reset link through email service. |
| `POST` | `/api/auth/forgot-password` | Public | Generates a reset token and sends a password reset link/message. |
| `POST` | `/api/auth/reset-password` | Public | Accepts email, reset code, and new password. |
| `GET` | `/api/auth/manage/profile` | Required | Returns current authenticated user's profile. |
| `PUT` | `/api/auth/manage/profile` | Required | Updates current authenticated user's first and last name. |
| `GET` | `/api/auth/manage/users` | Required, preferably `AdminOnly` | Lists registered users as safe response DTOs. |

The skill must create these files for the auth module:

```text
Endpoints/Auth/AuthEndpoints.cs
Endpoints/Auth/Models/RegisterUserRequest.cs
Endpoints/Auth/Models/ForgotPasswordRequest.cs
Endpoints/Auth/Models/ResetPasswordRequest.cs
Endpoints/Auth/Models/UpdateUserProfileRequest.cs
Endpoints/Auth/Models/UserProfileResponse.cs
```

If using the default Identity login from `MapIdentityApi<ApplicationUser>()`, do not implement a duplicate custom login unless the user requests a custom login response. Keep `/api/auth/login` visible in Swagger.

Expected Swagger group:

```text
Admin or Auth
```

Expected endpoint list after setup:

```text
POST /api/auth/login
POST /api/auth/register-admin
POST /api/auth/reset-password
POST /api/auth/forgot-password
GET  /api/auth/manage/profile
PUT  /api/auth/manage/profile
GET  /api/auth/manage/users
```

Example registration flow:

```csharp
private static async Task<IResult> RegisterUser(
    RegisterUserRequest dto,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IEmailSender emailSender,
    IConfiguration config)
{
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

    var tempPassword = "Admin123!";
    var created = await userManager.CreateAsync(user, tempPassword);

    if (!created.Succeeded)
    {
        return Results.BadRequest(new { Error = created.Errors });
    }

    if (await roleManager.RoleExistsAsync("User"))
    {
        await userManager.AddToRoleAsync(user, "User");
    }

    return Results.Ok(new { Message = $"User {user.Email} created." });
}
```

Important:

- Temporary passwords must satisfy Identity password rules.
- Do not return password hashes.
- Do not expose raw Identity user objects.
- Prefer response DTOs.

## DTOs

Create request/response models under the module that owns them.

Example:

```text
Endpoints/Auth/Models/RegisterUserRequest.cs
Endpoints/Auth/Models/UserProfileResponse.cs
```

Request example:

```csharp
public class RegisterUserRequest
{
    [Required]
    public string? Email { get; set; }

    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }
}
```

Response example:

```csharp
public class UserProfileResponse
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
}
```

## Services

Use services for reusable logic and external integrations.

Good service candidates:

- Email sending.
- File storage.
- External APIs.
- Payment integrations.
- Report generation.
- Reusable business rules.

Example:

```csharp
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ProjectName.Services;

public class ConsoleEmailService : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"To: {email}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine(htmlMessage);
        return Task.CompletedTask;
    }
}
```

Register it:

```csharp
builder.Services.AddTransient<IEmailSender, ConsoleEmailService>();
```

Lifetime guidance:

- Use `AddTransient` for lightweight stateless services.
- Use `AddScoped` for services that depend on `ApplicationDbContext`.
- Use `AddSingleton` only for safe shared stateless services.

## Middleware

Use middleware for behavior that applies before endpoints.

Common examples:

- Exception handling.
- Request logging.
- Blocking endpoints.
- Security headers.
- Timing requests.

Example:

```csharp
namespace ProjectName.Middleware;

public class BlockIdentityEndpoints
{
    private readonly RequestDelegate _next;

    private static readonly string[] BlockedPaths =
    [
        "/api/auth/register",
        "/api/auth/forgotpassword",
        "/api/auth/resetpassword"
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
```

Register it:

```csharp
app.UseMiddleware<BlockIdentityEndpoints>();
```

Order matters:

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BlockIdentityEndpoints>();
```

## Configuration

Use `appsettings.json`, user secrets, or environment variables.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mydb;Username=postgres;Password=secret"
  },
  "BaseURL": "https://localhost:7028"
}
```

Prefer user secrets for local sensitive values:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=mydb;Username=postgres;Password=secret"
```

Do not commit real secrets.

## Seed Roles

Create seed logic when roles are required.

```csharp
using Microsoft.AspNetCore.Identity;

namespace ProjectName.Data;

public static class SeedData
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = ["Admin", "User", "Researcher"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
```

Call it after building the app:

```csharp
await SeedData.SeedRolesAsync(app.Services);
```

## Migrations

After configuring Identity and `ApplicationDbContext`, create the first migration:

```bash
dotnet ef migrations add InitialIdentity
dotnet ef database update
```

If `dotnet ef` is missing:

```bash
dotnet tool install --global dotnet-ef
```

Only run database updates when the user expects schema changes.

## Validation

Always validate after changes:

```bash
dotnet build
```

If possible, also run:

```bash
dotnet test
```

Manual API checks:

1. Open Swagger.
2. Register a user.
3. Login.
4. Copy `accessToken`.
5. Click Swagger `Authorize`.
6. Paste token as Bearer token.
7. Test a protected endpoint.
8. Test an admin-only endpoint with and without the required role.

## Common Errors

### `PasswordRequiresDigit`

The password needs at least one number.

Use a password like:

```text
Admin123!
```

### `PasswordRequiresUpper`

The password needs at least one uppercase letter.

### `Failed to fetch` in Swagger

Likely causes:

- HTTPS dev certificate not trusted.
- Wrong port.
- Server is not running.
- CORS issue.
- Browser blocked the request before it reached the API.

Fix local HTTPS certificate:

```bash
dotnet dev-certs https --trust
```

### `401 Unauthorized`

The request has no token or the token is invalid/expired.

### `403 Forbidden`

The user is authenticated but does not have the required role/policy.

## Checklist

Complete these before considering the starter finished:

- Project structure inspected.
- Packages installed only if missing.
- `ApplicationUser` exists.
- `ApplicationDbContext` exists and inherits from `IdentityDbContext<ApplicationUser>`.
- Database provider configured.
- Connection string configured outside code when sensitive.
- Identity registered.
- Roles enabled with `.AddRoles<IdentityRole>()`.
- Swagger configured.
- Bearer auth available in Swagger.
- Services registered.
- Middleware registered only if needed.
- Authentication comes before authorization.
- Endpoint modules created.
- Custom auth/profile endpoints added if requested.
- Required starter endpoints appear in Swagger:
  - `POST /api/auth/login`
  - `POST /api/auth/register-admin`
  - `POST /api/auth/reset-password`
  - `POST /api/auth/forgot-password`
  - `GET /api/auth/manage/profile`
  - `PUT /api/auth/manage/profile`
  - `GET /api/auth/manage/users`
  - `GET /api/home/welcome`
- DTOs used for requests and responses.
- Seed roles added if roles are used.
- Migration created if requested.
- Database updated if requested.
- `dotnet build` passes.
- User can register.
- User can login.
- Protected endpoint works with token.
- Role-protected endpoint respects authorization.

## Implementation Rules

- Prefer existing project style over introducing a new style.
- Do not overwrite existing user files blindly.
- Keep `Program.cs` readable.
- Keep endpoint modules focused by feature.
- Keep services free of HTTP-specific details when possible.
- Keep middleware limited to cross-cutting concerns.
- Do not expose Identity internals in responses.
- Do not commit secrets.
- Explain any manual step the user must still perform, especially database credentials, certificate trust, or migrations.

## Definition Of Done

For a new API project, the skill is not complete until the project has:

- The expected folder structure.
- Auth endpoint module files.
- Home/welcome endpoint module.
- Identity configured and mapped under `/api/auth`.
- The required auth/profile endpoints visible in Swagger.
- Swagger Bearer authorization configured.
- Email sender service registered, even if it only writes to console.
- Middleware added when endpoint hiding or global error handling is requested.
- Role policy configured.
- Seed role logic included when roles are used.
- A successful `dotnet build`.

Do not stop after writing instructions. Implement the files and verify the project builds whenever the user asks to apply the starter to a project.

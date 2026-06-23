// Crea el constructor de la aplicación web con los argumentos de configuración
using AeonRegistryAPI.Endpoints.CustomIndentityEndpoints;
using AeonRegistryAPI.Endpoints.Home;
using AeonRegistryAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Agrega los servicios al contenedor de inyección de dependencias
// Obtén más información sobre cómo configurar OpenAPI en https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
// Registra el explorador de endpoints para descubrir las rutas de la API
//builder.Services.AddEndpointsApiExplorer();
// Registra el generador de Swagger para producir la documentación OpenAPI
//builder.Services.AddSwaggerGen();

builder.Services.AddcustomSwagger();

//get a connection to the database
var connectionString = DataUtility.GetConnectionString(builder.Configuration);

//Configure the database context for PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

//add indentity endpoints
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//Admin Policy
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

//Email Sender Service
builder.Services.AddTransient<IEmailSender, ConsoleEmailService>();

//enable validation for minimal APIs
builder.Services.AddValidation();

// Construye la aplicación web a partir de la configuración del builder
var app = builder.Build();

// Configura la canalización de procesamiento de solicitudes HTTP
// Si el entorno actual es Desarrollo
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    // Habilita el middleware que expone el documento Swagger
    app.UseSwagger();
    // Habilita la interfaz de usuario interactiva de Swagger
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    await DataSeed.ManageDataAsync(scope.ServiceProvider);
}

// Redirige automáticamente las solicitudes HTTP hacia HTTPS
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BlockIdentityEndpoints>();
/*
// Mapea un endpoint HTTP GET en la ruta /api/Welcome
app.MapGet("/api/Welcome", () =>
{
    // Crea un objeto anónimo con los datos de bienvenida
    var response = new
    {
        // Mensaje principal de bienvenida a la API
        Message = "Welcome to the Aeon Registry API!",
        // Versión actual de la API expuesta al cliente
        Version = "1.0.0",
        // Hora actual del servidor en formato corto
        TimeOnly = DateTime.Now.ToShortTimeString()
    };
    // Retorna la respuesta con código HTTP 200 OK
    return Results.Ok(response);

}).WithName("WelcomeMessage");
*/

var authRouteGroup = app.MapGroup("/api/auth")
    .WithTags("Admin");

authRouteGroup.MapIdentityApi<ApplicationUser>();

app.MapHomeEndpoints();
app.MapCustomIdentityEndpoints();
// Ejecuta la aplicación y comienza a escuchar solicitudes entrantes
app.Run();


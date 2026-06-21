// Crea el constructor de la aplicación web con los argumentos de configuración
var builder = WebApplication.CreateBuilder(args);

// Agrega los servicios al contenedor de inyección de dependencias
// Obtén más información sobre cómo configurar OpenAPI en https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
// Registra el explorador de endpoints para descubrir las rutas de la API
//builder.Services.AddEndpointsApiExplorer();
// Registra el generador de Swagger para producir la documentación OpenAPI
//builder.Services.AddSwaggerGen();

builder.Services.AddcustomSwagger();

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

// Redirige automáticamente las solicitudes HTTP hacia HTTPS
app.UseHttpsRedirection();
app.UseStaticFiles();
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


// Ejecuta la aplicación y comienza a escuchar solicitudes entrantes
app.Run();


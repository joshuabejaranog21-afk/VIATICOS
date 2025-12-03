using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.API.Services;
using OneCardExpenseValidator.Application.Services;
using OneCardExpenseValidator.Application.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configurar para escuchar en todas las interfaces (permite conexiones desde móviles)
builder.WebHost.UseUrls("http://0.0.0.0:5190", "https://0.0.0.0:5191");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Registrar servicios de OCR
builder.Services.AddScoped<IImagePreprocessingService, ImagePreprocessingService>();
builder.Services.AddScoped<IOcrService, TesseractOcrService>();
builder.Services.AddScoped<ITicketParserService, TicketParserService>();
builder.Services.AddScoped<IProductMatchingService, ProductMatchingService>();

// Registrar servicios de validación y categorización
builder.Services.AddScoped<ICategorizationService, CategorizationService>();
builder.Services.AddScoped<IClaudeService, ClaudeService>();
builder.Services.AddHttpClient(); // Para llamadas a Claude API

// Registrar servicios de notificaciones y límites de gasto
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISpendingLimitService, SpendingLimitService>();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar SignalR para comunicación en tiempo real
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Configurar CORS para permitir conexiones desde dispositivos móviles
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",
                "https://localhost:5001",
                "http://localhost:5190",
                "https://localhost:7190",
                "http://10.4.23.139:5190",
                "https://10.4.23.139:5191"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Configurar autorización
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicializar base de datos con datos iniciales
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comentado para permitir acceso HTTP desde dispositivos móviles
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Habilitar CORS
app.UseCors("SignalRPolicy");

// Agregar middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapear rutas de controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

// Mapear el Hub de SignalR para validación en tiempo real
app.MapHub<ValidationHub>("/validationHub");

app.Run();

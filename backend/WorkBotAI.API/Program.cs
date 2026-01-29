using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkBotAI.API.Data;
using WorkBotAI.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurazione Database
//builder.Services.AddDbContext<WorkBotAIContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<WorkBotAIContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// Registra AuthService
builder.Services.AddScoped<AuthService>();

// Configurazione JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "WorkBotAI_SuperSecretKey_2024_MinLength32Characters!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "WorkBotAI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "WorkBotAI",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Configurazione CORS per il frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Test endpoint per verificare connessione database
app.MapGet("/api/health", async (WorkBotAIContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var userCount = await db.Users.CountAsync();
        var tenantCount = await db.Tenants.CountAsync();
        
        return Results.Ok(new
        {
            status = "healthy",
            database = canConnect ? "connected" : "disconnected",
            users = userCount,
            tenants = tenantCount,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "unhealthy",
            database = "error",
            error = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
})
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
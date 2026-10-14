using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimpleCrm.Api.Data;
using SimpleCrm.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. Registrera tjänster (dependency injection) ----------

builder.Services.AddControllers()
    // Skicka enums som text i JSON ("Won" i stället för 3).
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI-beskrivning av API:t, finns på /openapi/v1.json i Development.
builder.Services.AddOpenApi();

// Koppla EF Core mot SQL Server. Anslutningssträngen ligger i appsettings.json.
builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CrmDb")));

builder.Services.AddScoped<TokenService>();

// JWT-inloggning: API:t godkänner anrop som har en giltig token i headern
// "Authorization: Bearer <token>".
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// CORS: tillåt frontenden (Vite på port 5173) att anropa API:t.
// I utveckling går anropen oftast via Vites proxy, men detta gör det möjligt
// att anropa API:t direkt också.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ---------- 2. Skapa/uppdatera databasen och lägg in testdata ----------

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    // Kör alla migrations som inte körts än (skapar databasen om den saknas).
    await db.Database.MigrateAsync();
    await SeedData.InitializeAsync(db);

    app.MapOpenApi();
}

// ---------- 3. Pipeline: i vilken ordning varje anrop hanteras ----------

app.UseCors("Frontend");
app.UseAuthentication(); // Vem är du? (läser token)
app.UseAuthorization();  // Får du göra detta? (kollar [Authorize])

app.MapControllers();

app.Run();

using System.Text;
using System.Text.Json.Serialization;
using DeskFlow.Infrastructure;
using DeskFlow.Domain.Services;
using DeskFlow.Api.Infrastructure;
using DeskFlow.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<SmartPriorityService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MetricsService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DeskFlow";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DeskFlow.Web";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    application = "DeskFlow",
    utc = DateTime.UtcNow
}));

app.MapControllers();

await app.Services.InitializeDatabaseAsync(builder.Configuration);

await app.RunAsync();

public partial class Program { }

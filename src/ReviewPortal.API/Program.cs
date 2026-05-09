using System.Security.Claims;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReviewPortal.API.Extensions;
using ReviewPortal.API.Middleware;
using ReviewPortal.Application.Common;

var builder = WebApplication.CreateBuilder(args);
AddLocalConfiguration(builder);
builder.Configuration.AddEnvironmentVariables();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
ValidateJwtSettings(jwtSettings);

// Add services to the container.
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ReviewPortal.API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Register Infrastructure (DbContext, repositories) and Application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

// Add CORS policy for Next.js frontend
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000", "https://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Use CORS before Authentication!
app.UseCors("NextJsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "ReviewPortal API",
    environment = app.Environment.EnvironmentName
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    checkedAtUtc = DateTime.UtcNow
}));

app.Run();

static void ValidateJwtSettings(JwtSettings settings)
{
    if (string.IsNullOrWhiteSpace(settings.Secret) || settings.Secret.Length < 32)
    {
        throw new InvalidOperationException("JWT configuration is missing a secure secret. Set Jwt:Secret via user secrets or environment variables.");
    }

    if (ContainsPlaceholder(settings.Secret))
    {
        throw new InvalidOperationException("JWT configuration still contains a placeholder value. Replace Jwt:Secret in user secrets, environment variables, or ignored appsettings.Local.json.");
    }

    if (string.IsNullOrWhiteSpace(settings.Issuer))
    {
        throw new InvalidOperationException("JWT configuration is missing Jwt:Issuer.");
    }

    if (ContainsPlaceholder(settings.Issuer))
    {
        throw new InvalidOperationException("JWT configuration still contains a placeholder issuer. Replace Jwt:Issuer in user secrets, environment variables, or ignored appsettings.Local.json.");
    }

    if (string.IsNullOrWhiteSpace(settings.Audience))
    {
        throw new InvalidOperationException("JWT configuration is missing Jwt:Audience.");
    }

    if (ContainsPlaceholder(settings.Audience))
    {
        throw new InvalidOperationException("JWT configuration still contains a placeholder audience. Replace Jwt:Audience in user secrets, environment variables, or ignored appsettings.Local.json.");
    }

    if (settings.ExpiryMinutes <= 0)
    {
        throw new InvalidOperationException("JWT configuration must use a positive ExpiryMinutes value.");
    }
}

static void AddLocalConfiguration(WebApplicationBuilder builder)
{
    const string localSettingsFile = "appsettings.Local.json";
    var localSettingsPath = Path.Combine(builder.Environment.ContentRootPath, localSettingsFile);

    if (!File.Exists(localSettingsPath))
    {
        return;
    }

    var localSettings = File.ReadAllText(localSettingsPath);
    if (ContainsPlaceholder(localSettings))
    {
        Console.WriteLine($"{localSettingsFile} contains placeholders and was not loaded. Replace placeholders before using it, or use user secrets/environment variables.");
        return;
    }

    builder.Configuration.AddJsonFile(localSettingsFile, optional: true, reloadOnChange: true);
}

static bool ContainsPlaceholder(string? value)
{
    return value?.Contains('<') == true || value?.Contains('>') == true;
}

public partial class Program
{
}

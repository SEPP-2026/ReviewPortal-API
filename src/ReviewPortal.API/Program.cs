using System.Security.Claims;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReviewPortal.API.Extensions;
using ReviewPortal.API.Middleware;
using ReviewPortal.Application.Common;
using ReviewPortal.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
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
var imageStorageOptions = builder.Configuration
    .GetSection(ImageStorageOptions.SectionName)
    .Get<ImageStorageOptions>() ?? new ImageStorageOptions();
var imageStorageRoot = Path.GetFullPath(Path.IsPathRooted(imageStorageOptions.RootPath)
    ? imageStorageOptions.RootPath
    : Path.Combine(app.Environment.ContentRootPath, imageStorageOptions.RootPath));
var imageRequestPath = NormalizeRequestPath(imageStorageOptions.RequestPath);
Directory.CreateDirectory(imageStorageRoot);

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

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imageStorageRoot),
    RequestPath = new PathString(imageRequestPath)
});

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

    if (string.IsNullOrWhiteSpace(settings.Issuer))
    {
        throw new InvalidOperationException("JWT configuration is missing Jwt:Issuer.");
    }

    if (string.IsNullOrWhiteSpace(settings.Audience))
    {
        throw new InvalidOperationException("JWT configuration is missing Jwt:Audience.");
    }

    if (settings.ExpiryMinutes <= 0)
    {
        throw new InvalidOperationException("JWT configuration must use a positive ExpiryMinutes value.");
    }
}

static string NormalizeRequestPath(string requestPath)
{
    if (string.IsNullOrWhiteSpace(requestPath))
    {
        return "/uploads/tools";
    }

    var normalized = requestPath.Replace('\\', '/').TrimEnd('/');
    return normalized.StartsWith('/')
        ? normalized
        : $"/{normalized}";
}

public partial class Program
{
}

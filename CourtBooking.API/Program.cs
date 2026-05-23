using CourtBooking.API.DependencyInjection;
using CourtBooking.API.Middleware;
using CourtBooking.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
var envPath = Path.Combine(builder.Environment.ContentRootPath, "..", ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}
else
{
    var fallbackEnv = Path.Combine(builder.Environment.ContentRootPath, ".env");
    if (File.Exists(fallbackEnv))
    {
        DotNetEnv.Env.Load(fallbackEnv);
    }
}

// Map .env values to configuration
builder.Configuration["DB_CONNECTION_STRING"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Configuration["JWT_KEY"] = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["JWT_KEY"];
builder.Configuration["JWT_ISSUER"] = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["JWT_ISSUER"];
builder.Configuration["JWT_AUDIENCE"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["JWT_AUDIENCE"];
builder.Configuration["JWT_ACCESS_TOKEN_MINUTES"] = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_MINUTES") ?? builder.Configuration["JWT_ACCESS_TOKEN_MINUTES"];
builder.Configuration["JWT_REFRESH_TOKEN_DAYS"] = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_DAYS") ?? builder.Configuration["JWT_REFRESH_TOKEN_DAYS"];
builder.Configuration["FRONTEND_URL"] = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? builder.Configuration["FRONTEND_URL"];

// Add services
builder.Services.AddControllers();
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddSwaggerWithJwt();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
var frontendUrl = builder.Configuration["FRONTEND_URL"] ?? "http://localhost:8100";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed data
await app.Services.UseInfrastructureAsync();

app.Run();

using CourtBooking.Application.Common.Interfaces;
using CourtBooking.Application.Features.Scoring;
using CourtBooking.Domain.Entities.Authentication;
using CourtBooking.Infrastructure.Authentication;
using CourtBooking.Infrastructure.Persistence;
using CourtBooking.Infrastructure.Seeding;
using CourtBooking.Infrastructure.Services.Scoring;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourtBooking.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DB_CONNECTION_STRING"]
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHttpClient();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IScoringMatchService, ScoringMatchService>();
        services.AddScoped<IScoringValidationService, ScoringValidationService>();
        services.AddScoped<IScoringEngineService, PickleballScoringEngine>();

        return services;
    }

    public static async Task UseInfrastructureAsync(this IServiceProvider serviceProvider)
    {
        await DataSeeder.SeedAsync(serviceProvider);
    }
}

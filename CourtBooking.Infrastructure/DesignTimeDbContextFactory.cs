using CourtBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CourtBooking.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Walk up from current directory to find .env
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        string? envPath = null;

        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate))
            {
                envPath = candidate;
                break;
            }
            dir = dir.Parent;
        }

        if (envPath != null)
        {
            DotNetEnv.Env.Load(envPath);
        }

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Data Source=localhost\\SQLEXPRESS;Initial Catalog=CourtBookingDb;Integrated Security=True;TrustServerCertificate=True;Application Name=CourtBooking;Command Timeout=0";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

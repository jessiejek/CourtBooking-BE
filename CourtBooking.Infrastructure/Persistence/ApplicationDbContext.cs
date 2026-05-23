using CourtBooking.Domain.Entities;
using CourtBooking.Domain.Entities.Authentication;
using CourtBooking.Domain.Entities.Scoring;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ScoreSport> ScoreSports => Set<ScoreSport>();
    public DbSet<ScoreRuleSet> ScoreRuleSets => Set<ScoreRuleSet>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ScoreSport>(entity =>
        {
            entity.ToTable("ScoreSports");
            entity.HasIndex(e => e.Code).IsUnique();
        });

        builder.Entity<ScoreRuleSet>(entity =>
        {
            entity.ToTable("ScoreRuleSets");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Sport)
                  .WithMany()
                  .HasForeignKey(e => e.SportId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AppSetting>(entity =>
        {
            entity.ToTable("AppSettings");
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }
}

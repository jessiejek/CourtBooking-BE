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
    public DbSet<ScoringMatch> ScoringMatches => Set<ScoringMatch>();
    public DbSet<ScoringTeam> ScoringTeams => Set<ScoringTeam>();
    public DbSet<ScoringPlayer> ScoringPlayers => Set<ScoringPlayer>();
    public DbSet<ScoringEvent> ScoringEvents => Set<ScoringEvent>();

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

        builder.Entity<ScoringMatch>(entity =>
        {
            entity.ToTable("ScoringMatches");
            entity.HasOne(e => e.Sport)
                  .WithMany()
                  .HasForeignKey(e => e.SportId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.RuleSet)
                  .WithMany()
                  .HasForeignKey(e => e.RuleSetId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => e.Status);
        });

        builder.Entity<ScoringTeam>(entity =>
        {
            entity.ToTable("ScoringTeams");
            entity.HasOne(e => e.Match)
                  .WithMany(m => m.Teams)
                  .HasForeignKey(e => e.MatchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ScoringPlayer>(entity =>
        {
            entity.ToTable("ScoringPlayers");
            entity.HasOne(e => e.Match)
                  .WithMany()
                  .HasForeignKey(e => e.MatchId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Players)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ScoringEvent>(entity =>
        {
            entity.ToTable("ScoringEvents");
            entity.HasOne(e => e.Match)
                  .WithMany(m => m.Events)
                  .HasForeignKey(e => e.MatchId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.SequenceNumber);
        });
    }
}

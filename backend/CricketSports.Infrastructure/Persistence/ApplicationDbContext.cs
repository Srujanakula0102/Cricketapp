using CricketSports.Infrastructure.Identity;
using CricketSports.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CricketSports.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentTeam> TournamentTeams => Set<TournamentTeam>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchTeam> MatchTeams => Set<MatchTeam>();
    public DbSet<Innings> Innings => Set<Innings>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().Property(user => user.DisplayName).HasMaxLength(100);

        builder.Entity<Team>(entity =>
        {
            entity.ToTable("Teams");
            entity.Property(team => team.Name).HasMaxLength(150);
            entity.Property(team => team.ShortName).HasMaxLength(10);
            entity.Property(team => team.CountryOrRegion).HasMaxLength(100);
            entity.Property(team => team.LogoUrl).HasMaxLength(500);
            entity.HasIndex(team => new { team.Name, team.CountryOrRegion }).IsUnique();
        });

        builder.Entity<Player>(entity =>
        {
            entity.ToTable("Players");
            entity.Property(player => player.FullName).HasMaxLength(150);
            entity.Property(player => player.CountryOrRegion).HasMaxLength(100);
            entity.Property(player => player.ProfileImageUrl).HasMaxLength(500);
            entity.Property(player => player.BattingStyle).HasMaxLength(80);
            entity.Property(player => player.BowlingStyle).HasMaxLength(80);
            entity.HasIndex(player => player.FullName);
            entity.HasOne(player => player.Team).WithMany(team => team.Players)
                .HasForeignKey(player => player.TeamId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Venue>(entity =>
        {
            entity.ToTable("Venues");
            entity.Property(venue => venue.Name).HasMaxLength(150);
            entity.Property(venue => venue.City).HasMaxLength(100);
            entity.Property(venue => venue.Country).HasMaxLength(100);
            entity.Property(venue => venue.ImageUrl).HasMaxLength(500);
            entity.HasIndex(venue => new { venue.Name, venue.City, venue.Country }).IsUnique();
        });

        builder.Entity<Tournament>(entity =>
        {
            entity.ToTable("Tournaments");
            entity.Property(tournament => tournament.Name).HasMaxLength(150);
            entity.Property(tournament => tournament.Season).HasMaxLength(30);
            entity.Property(tournament => tournament.LogoUrl).HasMaxLength(500);
            entity.HasIndex(tournament => new { tournament.Name, tournament.Season }).IsUnique();
        });

        builder.Entity<TournamentTeam>(entity =>
        {
            entity.ToTable("TournamentTeams");
            entity.HasKey(entry => new { entry.TournamentId, entry.TeamId });
            entity.HasOne(entry => entry.Tournament).WithMany(tournament => tournament.Teams)
                .HasForeignKey(entry => entry.TournamentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(entry => entry.Team).WithMany(team => team.TournamentEntries)
                .HasForeignKey(entry => entry.TeamId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Match>(entity =>
        {
            entity.ToTable("Matches");
            entity.Property(match => match.Notes).HasMaxLength(1000);
            entity.HasIndex(match => new { match.Status, match.StartsAt });
            entity.HasOne(match => match.Tournament).WithMany(tournament => tournament.Matches)
                .HasForeignKey(match => match.TournamentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(match => match.Venue).WithMany(venue => venue.Matches)
                .HasForeignKey(match => match.VenueId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<MatchTeam>(entity =>
        {
            entity.ToTable("MatchTeams");
            entity.HasKey(entry => new { entry.MatchId, entry.TeamId });
            entity.HasIndex(entry => new { entry.MatchId, entry.Role }).IsUnique();
            entity.HasOne(entry => entry.Match).WithMany(match => match.Teams)
                .HasForeignKey(entry => entry.MatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(entry => entry.Team).WithMany(team => team.MatchEntries)
                .HasForeignKey(entry => entry.TeamId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Innings>(entity =>
        {
            entity.ToTable("Innings");
            entity.HasIndex(innings => new { innings.MatchId, innings.Number }).IsUnique();
            entity.HasOne(innings => innings.Match).WithMany(match => match.Innings)
                .HasForeignKey(innings => innings.MatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(innings => innings.BattingTeam).WithMany()
                .HasForeignKey(innings => innings.BattingTeamId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Delivery>(entity =>
        {
            entity.ToTable("Deliveries");
            entity.Property(delivery => delivery.Commentary).HasMaxLength(1000);
            entity.HasIndex(delivery => new { delivery.InningsId, delivery.Sequence }).IsUnique();
            entity.HasOne(delivery => delivery.Innings).WithMany(innings => innings.Deliveries)
                .HasForeignKey(delivery => delivery.InningsId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

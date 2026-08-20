using CricketSports.Domain.Common;

namespace CricketSports.Domain.Entities;

public sealed class Team : Entity
{
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required string CountryOrRegion { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Player> Players { get; } = new List<Player>();
    public ICollection<TournamentTeam> TournamentEntries { get; } = new List<TournamentTeam>();
    public ICollection<MatchTeam> MatchEntries { get; } = new List<MatchTeam>();
}

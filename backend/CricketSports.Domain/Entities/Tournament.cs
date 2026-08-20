using CricketSports.Domain.Common;
using CricketSports.Domain.Enums;

namespace CricketSports.Domain.Entities;

public sealed class Tournament : Entity
{
    public required string Name { get; set; }
    public required string Season { get; set; }
    public MatchFormat Format { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? LogoUrl { get; set; }
    public ICollection<TournamentTeam> Teams { get; } = new List<TournamentTeam>();
    public ICollection<Match> Matches { get; } = new List<Match>();
}

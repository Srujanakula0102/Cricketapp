using CricketSports.Domain.Common;
using CricketSports.Domain.Enums;

namespace CricketSports.Domain.Entities;

public sealed class Match : Entity
{
    public Guid? TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public MatchFormat Format { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public int? OversLimit { get; set; }
    public string? Notes { get; set; }
    public ICollection<MatchTeam> Teams { get; } = new List<MatchTeam>();
    public ICollection<Innings> Innings { get; } = new List<Innings>();
}

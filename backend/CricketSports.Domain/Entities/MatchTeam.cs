using CricketSports.Domain.Enums;

namespace CricketSports.Domain.Entities;

public sealed class MatchTeam
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public MatchTeamRole Role { get; set; }
}

namespace CricketSports.Domain.Entities;

public sealed class TournamentTeam
{
    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
}

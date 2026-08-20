using CricketSports.Domain.Common;

namespace CricketSports.Domain.Entities;

public sealed class Innings : Entity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public Guid BattingTeamId { get; set; }
    public Team BattingTeam { get; set; } = null!;
    public int Number { get; set; }
    public int TotalRuns { get; set; }
    public int Wickets { get; set; }
    public int LegalBalls { get; set; }
    public bool IsComplete { get; set; }
    public Guid StrikerId { get; set; }
    public Guid NonStrikerId { get; set; }
    public Guid BowlerId { get; set; }
    public Guid OpeningStrikerId { get; set; }
    public Guid OpeningNonStrikerId { get; set; }
    public Guid OpeningBowlerId { get; set; }
    public ICollection<Delivery> Deliveries { get; } = new List<Delivery>();
}

using CricketSports.Domain.Common;
using CricketSports.Domain.Enums;

namespace CricketSports.Domain.Entities;

public sealed class Delivery : Entity
{
    public Guid InningsId { get; set; }
    public Innings Innings { get; set; } = null!;
    public int Sequence { get; set; }
    public int OverNumber { get; set; }
    public int BallNumber { get; set; }
    public Guid StrikerId { get; set; }
    public Guid NonStrikerId { get; set; }
    public Guid BowlerId { get; set; }
    public int RunsOffBat { get; set; }
    public int ExtraRuns { get; set; }
    public ExtraType ExtraType { get; set; }
    public bool IsWicket { get; set; }
    public WicketType? WicketType { get; set; }
    public Guid? DismissedPlayerId { get; set; }
    public Guid? IncomingBatterId { get; set; }
    public Guid StrikerAfterId { get; set; }
    public Guid NonStrikerAfterId { get; set; }
    public string? Commentary { get; set; }
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}

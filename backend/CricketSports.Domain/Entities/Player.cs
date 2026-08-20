using CricketSports.Domain.Common;
using CricketSports.Domain.Enums;

namespace CricketSports.Domain.Entities;

public sealed class Player : Entity
{
    public required string FullName { get; set; }
    public string? CountryOrRegion { get; set; }
    public string? ProfileImageUrl { get; set; }
    public PlayerRole Role { get; set; }
    public string? BattingStyle { get; set; }
    public string? BowlingStyle { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
}

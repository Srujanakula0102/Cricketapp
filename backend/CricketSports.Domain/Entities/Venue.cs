using CricketSports.Domain.Common;

namespace CricketSports.Domain.Entities;

public sealed class Venue : Entity
{
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public int? Capacity { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<Match> Matches { get; } = new List<Match>();
}

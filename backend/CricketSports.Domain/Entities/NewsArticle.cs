using CricketSports.Domain.Common;

namespace CricketSports.Domain.Entities;

public sealed class NewsArticle : Entity
{
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Summary { get; set; }
    public required string Content { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.UtcNow;
}

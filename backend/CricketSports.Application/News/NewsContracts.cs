namespace CricketSports.Application.News;

public sealed record NewsArticleRequest(string Title, string Summary, string Content, string? ImageUrl, bool IsFeatured, DateTimeOffset PublishedAt);
public sealed record NewsArticleResponse(Guid Id, string Title, string Slug, string Summary, string Content, string? ImageUrl, bool IsFeatured, DateTimeOffset PublishedAt);

public interface INewsService
{
    Task<IReadOnlyCollection<NewsArticleResponse>> GetArticlesAsync(int take, CancellationToken cancellationToken);
    Task<NewsArticleResponse?> GetArticleAsync(string slug, CancellationToken cancellationToken);
    Task<NewsArticleResponse?> CreateAsync(NewsArticleRequest request, CancellationToken cancellationToken);
    Task<NewsArticleResponse?> UpdateAsync(Guid id, NewsArticleRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

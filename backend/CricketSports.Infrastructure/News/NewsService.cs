using System.Text.RegularExpressions;
using CricketSports.Application.News;
using CricketSports.Domain.Entities;
using CricketSports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CricketSports.Infrastructure.News;

public sealed class NewsService(ApplicationDbContext database) : INewsService
{
    public async Task<IReadOnlyCollection<NewsArticleResponse>> GetArticlesAsync(int take, CancellationToken ct)
        => await database.NewsArticles.AsNoTracking().OrderByDescending(article => article.PublishedAt).Take(Math.Clamp(take, 1, 100)).Select(article => ToResponse(article)).ToListAsync(ct);

    public Task<NewsArticleResponse?> GetArticleAsync(string slug, CancellationToken ct)
        => database.NewsArticles.AsNoTracking().Where(article => article.Slug == slug).Select(article => ToResponse(article)).SingleOrDefaultAsync(ct);

    public async Task<NewsArticleResponse?> CreateAsync(NewsArticleRequest request, CancellationToken ct)
    {
        var slug = await UniqueSlugAsync(Slugify(request.Title), null, ct); if (slug is null) return null;
        var article = new NewsArticle { Title = request.Title.Trim(), Slug = slug, Summary = request.Summary.Trim(), Content = request.Content.Trim(), ImageUrl = request.ImageUrl?.Trim(), IsFeatured = request.IsFeatured, PublishedAt = request.PublishedAt };
        database.NewsArticles.Add(article); await database.SaveChangesAsync(ct); return ToResponse(article);
    }

    public async Task<NewsArticleResponse?> UpdateAsync(Guid id, NewsArticleRequest request, CancellationToken ct)
    {
        var article = await database.NewsArticles.FindAsync([id], ct); if (article is null) return null;
        var slug = await UniqueSlugAsync(Slugify(request.Title), id, ct); if (slug is null) return null;
        article.Title = request.Title.Trim(); article.Slug = slug; article.Summary = request.Summary.Trim(); article.Content = request.Content.Trim(); article.ImageUrl = request.ImageUrl?.Trim(); article.IsFeatured = request.IsFeatured; article.PublishedAt = request.PublishedAt;
        await database.SaveChangesAsync(ct); return ToResponse(article);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) { var article = await database.NewsArticles.FindAsync([id], ct); if (article is null) return false; database.NewsArticles.Remove(article); await database.SaveChangesAsync(ct); return true; }
    private async Task<string?> UniqueSlugAsync(string baseSlug, Guid? currentId, CancellationToken ct) { for (var suffix = 0; suffix < 100; suffix++) { var candidate = suffix == 0 ? baseSlug : $"{baseSlug}-{suffix + 1}"; if (!await database.NewsArticles.AnyAsync(article => article.Slug == candidate && article.Id != currentId, ct)) return candidate; } return null; }
    private static string Slugify(string title) { var slug = Regex.Replace(title.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-'); return string.IsNullOrWhiteSpace(slug) ? "article" : slug; }
    private static NewsArticleResponse ToResponse(NewsArticle article) => new(article.Id, article.Title, article.Slug, article.Summary, article.Content, article.ImageUrl, article.IsFeatured, article.PublishedAt);
}

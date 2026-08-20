using CricketSports.Application.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CricketSports.API.Controllers;

[ApiController]
[Route("api/news")]
public sealed class NewsController(INewsService news) : ControllerBase
{
    [HttpGet, AllowAnonymous] public Task<IReadOnlyCollection<NewsArticleResponse>> Get(int take = 20, CancellationToken ct = default) => news.GetArticlesAsync(take, ct);
    [HttpGet("{slug}"), AllowAnonymous] public async Task<ActionResult<NewsArticleResponse>> GetBySlug(string slug, CancellationToken ct) => await news.GetArticleAsync(slug, ct) is { } article ? Ok(article) : NotFound();
    [HttpPost, Authorize(Roles = "Admin")] public async Task<ActionResult<NewsArticleResponse>> Post(NewsArticleRequest request, CancellationToken ct) => await news.CreateAsync(request, ct) is { } article ? CreatedAtAction(nameof(GetBySlug), new { article.Slug }, article) : Conflict("Unable to assign a unique article slug.");
    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")] public async Task<ActionResult<NewsArticleResponse>> Put(Guid id, NewsArticleRequest request, CancellationToken ct) => await news.UpdateAsync(id, request, ct) is { } article ? Ok(article) : NotFound();
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await news.DeleteAsync(id, ct) ? NoContent() : NotFound();
}

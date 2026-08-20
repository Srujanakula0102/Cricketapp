using CricketSports.Application.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CricketSports.API.Controllers;
[ApiController, Route("api/players")]
public sealed class PlayersController(ICatalogService service) : ControllerBase
{
    [HttpGet, AllowAnonymous] public Task<PagedResult<PlayerResponse>> Get(string? search, int page = 1, int pageSize = 20, CancellationToken ct = default) => service.GetPlayersAsync(search, page, pageSize, ct);
    [HttpGet("{id:guid}"), AllowAnonymous] public async Task<ActionResult<PlayerResponse>> Get(Guid id, CancellationToken ct) => await service.GetPlayerAsync(id, ct) is { } item ? Ok(item) : NotFound();
    [HttpPost, Authorize(Roles = "Admin")] public async Task<ActionResult<PlayerResponse>> Post(PlayerRequest request, CancellationToken ct) => await service.CreatePlayerAsync(request, ct) is { } item ? CreatedAtAction(nameof(Get), new { item.Id }, item) : BadRequest("The selected team does not exist.");
    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")] public async Task<ActionResult<PlayerResponse>> Put(Guid id, PlayerRequest request, CancellationToken ct) => await service.UpdatePlayerAsync(id, request, ct) is { } item ? Ok(item) : NotFound();
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await service.DeletePlayerAsync(id, ct) ? NoContent() : NotFound();
}

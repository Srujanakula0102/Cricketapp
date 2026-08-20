using CricketSports.Application.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CricketSports.API.Controllers;
[ApiController, Route("api/venues")]
public sealed class VenuesController(ICatalogService service) : ControllerBase
{
    [HttpGet, AllowAnonymous] public Task<PagedResult<VenueResponse>> Get(string? search, int page = 1, int pageSize = 20, CancellationToken ct = default) => service.GetVenuesAsync(search, page, pageSize, ct);
    [HttpGet("{id:guid}"), AllowAnonymous] public async Task<ActionResult<VenueResponse>> Get(Guid id, CancellationToken ct) => await service.GetVenueAsync(id, ct) is { } item ? Ok(item) : NotFound();
    [HttpPost, Authorize(Roles = "Admin")] public async Task<ActionResult<VenueResponse>> Post(VenueRequest request, CancellationToken ct) => await service.CreateVenueAsync(request, ct) is { } item ? CreatedAtAction(nameof(Get), new { item.Id }, item) : BadRequest("Capacity cannot be negative.");
    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")] public async Task<ActionResult<VenueResponse>> Put(Guid id, VenueRequest request, CancellationToken ct) => await service.UpdateVenueAsync(id, request, ct) is { } item ? Ok(item) : NotFound();
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await service.DeleteVenueAsync(id, ct) ? NoContent() : NotFound();
}

using CricketSports.Application.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CricketSports.API.Controllers;

[ApiController]
[Route("api/teams")]
public sealed class TeamsController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public Task<PagedResult<TeamResponse>> Get(string? search, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) => catalogService.GetTeamsAsync(search, page, pageSize, cancellationToken);

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<TeamResponse>> GetById(Guid id, CancellationToken cancellationToken)
        => await catalogService.GetTeamAsync(id, cancellationToken) is { } team ? Ok(team) : NotFound();

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TeamResponse>> Create(TeamRequest request, CancellationToken cancellationToken)
    {
        var team = await catalogService.CreateTeamAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { team.Id }, team);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TeamResponse>> Update(Guid id, TeamRequest request, CancellationToken cancellationToken)
        => await catalogService.UpdateTeamAsync(id, request, cancellationToken) is { } team ? Ok(team) : NotFound();

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await catalogService.DeleteTeamAsync(id, cancellationToken) ? NoContent() : NotFound();
}

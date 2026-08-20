using CricketSports.Application.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CricketSports.API.Controllers;
[ApiController,Route("api/matches")] public sealed class MatchesController(ICatalogService s):ControllerBase
{
 [HttpGet,AllowAnonymous] public Task<PagedResult<MatchResponse>> Get(int page=1,int pageSize=20,CancellationToken ct=default)=>s.GetMatchesAsync(page,pageSize,ct);
 [HttpGet("{id:guid}"),AllowAnonymous] public async Task<ActionResult<MatchResponse>> Get(Guid id,CancellationToken ct)=>await s.GetMatchAsync(id,ct)is{} x?Ok(x):NotFound();
 [HttpPost,Authorize(Roles="Admin")] public async Task<ActionResult<MatchResponse>> Post(MatchRequest r,CancellationToken ct)=>await s.CreateMatchAsync(r,ct)is{} x?CreatedAtAction(nameof(Get),new{x.Id},x):BadRequest("Teams, venue, tournament, or match limits are invalid.");
 [HttpPut("{id:guid}"),Authorize(Roles="Admin")] public async Task<ActionResult<MatchResponse>> Put(Guid id,MatchRequest r,CancellationToken ct)=>await s.UpdateMatchAsync(id,r,ct)is{} x?Ok(x):NotFound();
 [HttpDelete("{id:guid}"),Authorize(Roles="Admin")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct)=>await s.DeleteMatchAsync(id,ct)?NoContent():NotFound();
}

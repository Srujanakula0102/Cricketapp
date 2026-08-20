using CricketSports.Application.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CricketSports.API.Controllers;
[ApiController,Route("api/tournaments")] public sealed class TournamentsController(ICatalogService s):ControllerBase
{
 [HttpGet,AllowAnonymous] public Task<PagedResult<TournamentResponse>> Get(int page=1,int pageSize=20,CancellationToken ct=default)=>s.GetTournamentsAsync(page,pageSize,ct);
 [HttpGet("{id:guid}"),AllowAnonymous] public async Task<ActionResult<TournamentResponse>> Get(Guid id,CancellationToken ct)=>await s.GetTournamentAsync(id,ct)is{} x?Ok(x):NotFound();
 [HttpPost,Authorize(Roles="Admin")] public async Task<ActionResult<TournamentResponse>> Post(TournamentRequest r,CancellationToken ct)=>await s.CreateTournamentAsync(r,ct)is{} x?CreatedAtAction(nameof(Get),new{x.Id},x):BadRequest("Dates or team references are invalid.");
 [HttpPut("{id:guid}"),Authorize(Roles="Admin")] public async Task<ActionResult<TournamentResponse>> Put(Guid id,TournamentRequest r,CancellationToken ct)=>await s.UpdateTournamentAsync(id,r,ct)is{} x?Ok(x):NotFound();
 [HttpDelete("{id:guid}"),Authorize(Roles="Admin")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct)=>await s.DeleteTournamentAsync(id,ct)?NoContent():NotFound();
}

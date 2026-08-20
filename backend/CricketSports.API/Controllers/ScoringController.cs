using CricketSports.Application.Scoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CricketSports.API.Controllers;

[ApiController]
[Route("api/scoring/matches/{matchId:guid}")]
[Authorize(Roles = "Admin,Scorer")]
public sealed class ScoringController(IScoringService scoring) : ControllerBase
{
    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<ActionResult<InningsStateResponse>> Current(Guid matchId, CancellationToken ct)
        => await scoring.GetCurrentInningsAsync(matchId, ct) is { } result ? Ok(result) : NotFound();

    [HttpGet("scorecard")]
    [AllowAnonymous]
    public Task<IReadOnlyCollection<InningsScorecardResponse>> Scorecard(Guid matchId, CancellationToken ct)
        => scoring.GetScorecardAsync(matchId, ct);

    [HttpGet("commentary")]
    [AllowAnonymous]
    public Task<IReadOnlyCollection<CommentaryResponse>> Commentary(Guid matchId, CancellationToken ct)
        => scoring.GetCommentaryAsync(matchId, ct);

    [HttpPost("innings/start")]
    public async Task<ActionResult<InningsStateResponse>> StartInnings(Guid matchId, StartInningsRequest request, CancellationToken ct)
        => await scoring.StartInningsAsync(matchId, request, ct) is { } result ? Ok(result) : BadRequest("Unable to start innings. Verify match state and selected players.");

    [HttpPost("delivery")]
    public async Task<ActionResult<InningsStateResponse>> Delivery(Guid matchId, DeliveryRequest request, CancellationToken ct)
        => await scoring.RecordDeliveryAsync(matchId, request, ct) is { } result ? Ok(result) : BadRequest("Invalid delivery for the current innings state.");

    [HttpPost("bowler")]
    public async Task<ActionResult<InningsStateResponse>> ChangeBowler(Guid matchId, ChangeBowlerRequest request, CancellationToken ct)
        => await scoring.ChangeBowlerAsync(matchId, request, ct) is { } result ? Ok(result) : BadRequest("A different eligible bowler can only be selected between overs.");

    [HttpPost("undo")]
    public async Task<ActionResult<InningsStateResponse>> Undo(Guid matchId, CancellationToken ct)
        => await scoring.UndoLastDeliveryAsync(matchId, ct) is { } result ? Ok(result) : BadRequest("No delivery is available to undo.");

    [HttpPost("innings/end")]
    public async Task<ActionResult<InningsStateResponse>> EndInnings(Guid matchId, CancellationToken ct)
        => await scoring.EndInningsAsync(matchId, ct) is { } result ? Ok(result) : BadRequest("No active innings is available to end.");

    [HttpPost("end")]
    public async Task<IActionResult> EndMatch(Guid matchId, CancellationToken ct)
        => await scoring.EndMatchAsync(matchId, ct) ? NoContent() : BadRequest("Only a live match can be ended.");
}

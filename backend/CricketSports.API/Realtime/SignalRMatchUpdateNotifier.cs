using CricketSports.Application.Scoring;
using Microsoft.AspNetCore.SignalR;

namespace CricketSports.API.Realtime;

public sealed class SignalRMatchUpdateNotifier(IHubContext<LiveMatchHub> hub) : IMatchUpdateNotifier
{
    public Task ScoreUpdatedAsync(Guid matchId, InningsStateResponse state, CancellationToken ct)
        => hub.Clients.Group(LiveMatchHub.GroupName(matchId)).SendAsync("ScoreUpdated", state, ct);
    public Task DeliveryRecordedAsync(Guid matchId, DeliveryLiveEvent delivery, CancellationToken ct)
        => hub.Clients.Group(LiveMatchHub.GroupName(matchId)).SendAsync("DeliveryRecorded", delivery, ct);
    public Task InningsCompletedAsync(Guid matchId, InningsStateResponse state, CancellationToken ct)
        => hub.Clients.Group(LiveMatchHub.GroupName(matchId)).SendAsync("InningsCompleted", state, ct);
    public Task MatchCompletedAsync(Guid matchId, CancellationToken ct)
        => hub.Clients.Group(LiveMatchHub.GroupName(matchId)).SendAsync("MatchCompleted", new { matchId }, ct);
}

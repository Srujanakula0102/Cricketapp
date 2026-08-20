using Microsoft.AspNetCore.SignalR;

namespace CricketSports.API.Realtime;

public sealed class LiveMatchHub : Hub
{
    public Task JoinMatch(string matchId)
        => Guid.TryParse(matchId, out var id)
            ? Groups.AddToGroupAsync(Context.ConnectionId, GroupName(id))
            : throw new HubException("A valid match ID is required.");

    public Task LeaveMatch(string matchId)
        => Guid.TryParse(matchId, out var id)
            ? Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(id))
            : Task.CompletedTask;

    public static string GroupName(Guid matchId) => $"match-{matchId}";
}

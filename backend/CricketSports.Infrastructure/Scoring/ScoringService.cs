using CricketSports.Application.Scoring;
using CricketSports.Domain.Entities;
using CricketSports.Domain.Enums;
using CricketSports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CricketSports.Infrastructure.Scoring;

public sealed class ScoringService(ApplicationDbContext database, IMatchUpdateNotifier notifier) : IScoringService
{
    public async Task<InningsStateResponse?> GetCurrentInningsAsync(Guid matchId, CancellationToken ct)
    {
        var innings = await database.Innings.AsNoTracking().Include(item => item.Deliveries)
            .SingleOrDefaultAsync(item => item.MatchId == matchId && !item.IsComplete, ct);
        return innings is null ? null : State(innings);
    }

    public async Task<IReadOnlyCollection<InningsScorecardResponse>> GetScorecardAsync(Guid matchId, CancellationToken ct)
    {
        var innings = await database.Innings.AsNoTracking().Include(item => item.BattingTeam).Include(item => item.Deliveries)
            .Where(item => item.MatchId == matchId).OrderBy(item => item.Number).ToListAsync(ct);
        var playerIds = innings.SelectMany(item => item.Deliveries.SelectMany(delivery => new[] { delivery.StrikerId, delivery.BowlerId, delivery.DismissedPlayerId ?? Guid.Empty }))
            .Concat(innings.SelectMany(item => new[] { item.OpeningStrikerId, item.OpeningNonStrikerId, item.OpeningBowlerId })).Where(id => id != Guid.Empty).Distinct().ToArray();
        var names = await database.Players.AsNoTracking().Where(player => playerIds.Contains(player.Id)).ToDictionaryAsync(player => player.Id, player => player.FullName, ct);
        string Name(Guid id) => names.TryGetValue(id, out var name) ? name : "Unknown player";
        return innings.Select(item =>
        {
            var batterIds = item.Deliveries.Select(delivery => delivery.StrikerId).Append(item.OpeningStrikerId).Append(item.OpeningNonStrikerId).Concat(item.Deliveries.Where(delivery => delivery.DismissedPlayerId.HasValue).Select(delivery => delivery.DismissedPlayerId!.Value)).Distinct();
            var batting = batterIds.Select(id => new BattingScorecardResponse(Name(id), item.Deliveries.Where(delivery => delivery.StrikerId == id).Sum(delivery => delivery.RunsOffBat), item.Deliveries.Count(delivery => delivery.StrikerId == id && delivery.ExtraType is not ExtraType.Wide and not ExtraType.NoBall), item.Deliveries.Count(delivery => delivery.StrikerId == id && delivery.RunsOffBat == 4), item.Deliveries.Count(delivery => delivery.StrikerId == id && delivery.RunsOffBat == 6), item.Deliveries.Any(delivery => delivery.DismissedPlayerId == id))).ToList();
            var bowling = item.Deliveries.Select(delivery => delivery.BowlerId).Append(item.OpeningBowlerId).Distinct().Select(id => new BowlingScorecardResponse(Name(id), item.Deliveries.Count(delivery => delivery.BowlerId == id && delivery.ExtraType is not ExtraType.Wide and not ExtraType.NoBall), item.Deliveries.Where(delivery => delivery.BowlerId == id).Sum(delivery => delivery.RunsOffBat + (delivery.ExtraType is ExtraType.Wide or ExtraType.NoBall ? delivery.ExtraRuns : 0)), item.Deliveries.Count(delivery => delivery.BowlerId == id && delivery.IsWicket && delivery.WicketType is not WicketType.RunOut and not WicketType.RetiredHurt))).ToList();
            return new InningsScorecardResponse(item.Number, item.BattingTeam.Name, item.TotalRuns, item.Wickets, item.LegalBalls, item.IsComplete, item.Deliveries.Sum(delivery => delivery.ExtraRuns), batting, bowling);
        }).ToList();
    }

    public async Task<IReadOnlyCollection<CommentaryResponse>> GetCommentaryAsync(Guid matchId, CancellationToken ct)
        => await database.Deliveries.AsNoTracking().Where(delivery => delivery.Innings.MatchId == matchId)
            .OrderByDescending(delivery => delivery.Innings.Number).ThenByDescending(delivery => delivery.Sequence)
            .Select(delivery => new CommentaryResponse(delivery.Innings.Number, delivery.OverNumber, delivery.BallNumber, delivery.RunsOffBat, delivery.ExtraRuns, delivery.ExtraType, delivery.IsWicket, delivery.Commentary, delivery.RecordedAt)).ToListAsync(ct);

    public async Task<InningsStateResponse?> StartInningsAsync(Guid matchId, StartInningsRequest request, CancellationToken ct)
    {
        var match = await database.Matches.Include(match => match.Teams).Include(match => match.Innings).SingleOrDefaultAsync(match => match.Id == matchId, ct);
        if (match is null || match.Innings.Any(innings => !innings.IsComplete) || !match.Teams.Any(team => team.TeamId == request.BattingTeamId)) return null;
        var fieldingTeamId = match.Teams.Single(team => team.TeamId != request.BattingTeamId).TeamId;
        if (request.StrikerId == request.NonStrikerId || !await PlayersBelongToTeams(request.BattingTeamId, [request.StrikerId, request.NonStrikerId], ct) || !await PlayersBelongToTeams(fieldingTeamId, [request.BowlerId], ct)) return null;
        var innings = new Innings { MatchId = matchId, BattingTeamId = request.BattingTeamId, Number = match.Innings.Count + 1, StrikerId = request.StrikerId, NonStrikerId = request.NonStrikerId, BowlerId = request.BowlerId, OpeningStrikerId = request.StrikerId, OpeningNonStrikerId = request.NonStrikerId, OpeningBowlerId = request.BowlerId };
        match.Status = MatchStatus.Live; database.Innings.Add(innings); await database.SaveChangesAsync(ct); var state = State(innings); await notifier.ScoreUpdatedAsync(matchId, state, ct); return state;
    }

    public async Task<InningsStateResponse?> RecordDeliveryAsync(Guid matchId, DeliveryRequest request, CancellationToken ct)
    {
        if (request.RunsOffBat < 0 || request.ExtraRuns < 0 || (request.ExtraType == ExtraType.None && request.ExtraRuns != 0) || (request.ExtraType != ExtraType.None && request.ExtraRuns == 0)) return null;
        var innings = await ActiveInnings(matchId, ct); if (innings is null) return null;
        if (request.IsWicket != request.WicketType.HasValue || (request.IsWicket && request.IncomingBatterId is null)) return null;
        if (request.DismissedPlayerId.HasValue && request.DismissedPlayerId != innings.StrikerId && request.DismissedPlayerId != innings.NonStrikerId) return null;
        if (request.IsWicket && (request.IncomingBatterId == innings.StrikerId || request.IncomingBatterId == innings.NonStrikerId)) return null;
        if (request.IsWicket && !await PlayersBelongToTeams(innings.BattingTeamId, [request.IncomingBatterId!.Value], ct)) return null;
        var isLegal = request.ExtraType is not ExtraType.Wide and not ExtraType.NoBall;
        var striker = innings.StrikerId; var nonStriker = innings.NonStrikerId;
        if (request.IsWicket) { if ((request.DismissedPlayerId ?? striker) == striker) striker = request.IncomingBatterId!.Value; else nonStriker = request.IncomingBatterId!.Value; innings.Wickets++; }
        var total = request.RunsOffBat + request.ExtraRuns;
        if (total % 2 == 1) (striker, nonStriker) = (nonStriker, striker);
        var nextLegalBalls = innings.LegalBalls + (isLegal ? 1 : 0);
        if (isLegal && nextLegalBalls % 6 == 0) (striker, nonStriker) = (nonStriker, striker);
        var delivery = new Delivery { InningsId = innings.Id, Sequence = innings.Deliveries.Count + 1, OverNumber = innings.LegalBalls / 6, BallNumber = innings.LegalBalls % 6 + 1, StrikerId = innings.StrikerId, NonStrikerId = innings.NonStrikerId, BowlerId = innings.BowlerId, RunsOffBat = request.RunsOffBat, ExtraRuns = request.ExtraRuns, ExtraType = request.ExtraType, IsWicket = request.IsWicket, WicketType = request.WicketType, DismissedPlayerId = request.DismissedPlayerId ?? (request.IsWicket ? innings.StrikerId : null), IncomingBatterId = request.IncomingBatterId, StrikerAfterId = striker, NonStrikerAfterId = nonStriker, Commentary = request.Commentary?.Trim() };
        innings.TotalRuns += total; innings.LegalBalls = nextLegalBalls; innings.StrikerId = striker; innings.NonStrikerId = nonStriker; database.Deliveries.Add(delivery); await database.SaveChangesAsync(ct); var state = State(innings, delivery.Sequence); await notifier.DeliveryRecordedAsync(matchId, new DeliveryLiveEvent(state, delivery.RunsOffBat, delivery.ExtraRuns, delivery.ExtraType, delivery.IsWicket), ct); await notifier.ScoreUpdatedAsync(matchId, state, ct); return state;
    }

    public async Task<InningsStateResponse?> UndoLastDeliveryAsync(Guid matchId, CancellationToken ct)
    {
        var innings = await ActiveInnings(matchId, ct); if (innings is null) return null;
        var last = await database.Deliveries.Where(delivery => delivery.InningsId == innings.Id).OrderByDescending(delivery => delivery.Sequence).FirstOrDefaultAsync(ct); if (last is null) return null;
        database.Deliveries.Remove(last); await database.SaveChangesAsync(ct);
        var previous = await database.Deliveries.Where(delivery => delivery.InningsId == innings.Id).OrderByDescending(delivery => delivery.Sequence).FirstOrDefaultAsync(ct);
        innings.TotalRuns -= last.RunsOffBat + last.ExtraRuns; innings.Wickets -= last.IsWicket ? 1 : 0; innings.LegalBalls -= last.ExtraType is ExtraType.Wide or ExtraType.NoBall ? 0 : 1; innings.StrikerId = previous?.StrikerAfterId ?? innings.OpeningStrikerId; innings.NonStrikerId = previous?.NonStrikerAfterId ?? innings.OpeningNonStrikerId;
        await database.SaveChangesAsync(ct); var state = State(innings, previous?.Sequence ?? 0); await notifier.ScoreUpdatedAsync(matchId, state, ct); return state;
    }

    public async Task<InningsStateResponse?> ChangeBowlerAsync(Guid matchId, ChangeBowlerRequest request, CancellationToken ct)
    {
        var innings = await ActiveInnings(matchId, ct); if (innings is null || innings.LegalBalls % 6 != 0) return null;
        var fieldingTeam = await database.MatchTeams.Where(entry => entry.MatchId == matchId && entry.TeamId != innings.BattingTeamId).Select(entry => entry.TeamId).SingleAsync(ct);
        if (request.BowlerId == innings.BowlerId || !await PlayersBelongToTeams(fieldingTeam, [request.BowlerId], ct)) return null;
        innings.BowlerId = request.BowlerId; await database.SaveChangesAsync(ct); var state = State(innings); await notifier.ScoreUpdatedAsync(matchId, state, ct); return state;
    }

    public async Task<InningsStateResponse?> EndInningsAsync(Guid matchId, CancellationToken ct)
    {
        var innings = await ActiveInnings(matchId, ct); if (innings is null) return null;
        innings.IsComplete = true; await database.SaveChangesAsync(ct); var state = State(innings); await notifier.InningsCompletedAsync(matchId, state, ct); return state;
    }

    public async Task<bool> EndMatchAsync(Guid matchId, CancellationToken ct)
    {
        var match = await database.Matches.SingleOrDefaultAsync(item => item.Id == matchId, ct);
        if (match is null || match.Status != MatchStatus.Live) return false;
        match.Status = MatchStatus.Completed; await database.SaveChangesAsync(ct); await notifier.MatchCompletedAsync(matchId, ct); return true;
    }

    private Task<Innings?> ActiveInnings(Guid matchId, CancellationToken ct) => database.Innings.Include(innings => innings.Deliveries).SingleOrDefaultAsync(innings => innings.MatchId == matchId && !innings.IsComplete, ct);
    private async Task<bool> PlayersBelongToTeams(Guid teamId, IReadOnlyCollection<Guid> players, CancellationToken ct)
    {
        var requiredPlayers = players.Distinct().ToArray();
        return await database.Players.CountAsync(player => player.TeamId == teamId && requiredPlayers.Contains(player.Id), ct) == requiredPlayers.Length;
    }
    private static InningsStateResponse State(Innings innings, int? sequence = null) => new(innings.Id, innings.TotalRuns, innings.Wickets, innings.LegalBalls, innings.LegalBalls / 6, innings.LegalBalls % 6, innings.StrikerId, innings.NonStrikerId, innings.BowlerId, innings.IsComplete, sequence ?? innings.Deliveries.Count);
}

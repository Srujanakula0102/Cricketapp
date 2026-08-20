using CricketSports.Domain.Enums;

namespace CricketSports.Application.Scoring;

public sealed record StartInningsRequest(Guid BattingTeamId, Guid StrikerId, Guid NonStrikerId, Guid BowlerId);
public sealed record DeliveryRequest(int RunsOffBat, int ExtraRuns, ExtraType ExtraType, bool IsWicket, WicketType? WicketType, Guid? DismissedPlayerId, Guid? IncomingBatterId, string? Commentary);
public sealed record ChangeBowlerRequest(Guid BowlerId);
public sealed record InningsStateResponse(Guid InningsId, int TotalRuns, int Wickets, int LegalBalls, int OverNumber, int BallInOver, Guid StrikerId, Guid NonStrikerId, Guid BowlerId, bool IsComplete, int EventSequence);
public sealed record DeliveryLiveEvent(InningsStateResponse State, int RunsOffBat, int ExtraRuns, ExtraType ExtraType, bool IsWicket);
public sealed record BattingScorecardResponse(string PlayerName, int Runs, int Balls, int Fours, int Sixes, bool IsOut);
public sealed record BowlingScorecardResponse(string PlayerName, int LegalBalls, int RunsConceded, int Wickets);
public sealed record InningsScorecardResponse(int Number, string BattingTeamName, int TotalRuns, int Wickets, int LegalBalls, bool IsComplete, int Extras, IReadOnlyCollection<BattingScorecardResponse> Batting, IReadOnlyCollection<BowlingScorecardResponse> Bowling);
public sealed record CommentaryResponse(int InningsNumber, int OverNumber, int BallNumber, int RunsOffBat, int ExtraRuns, ExtraType ExtraType, bool IsWicket, string? Commentary, DateTimeOffset RecordedAt);

public interface IMatchUpdateNotifier
{
    Task ScoreUpdatedAsync(Guid matchId, InningsStateResponse state, CancellationToken cancellationToken);
    Task DeliveryRecordedAsync(Guid matchId, DeliveryLiveEvent delivery, CancellationToken cancellationToken);
    Task InningsCompletedAsync(Guid matchId, InningsStateResponse state, CancellationToken cancellationToken);
    Task MatchCompletedAsync(Guid matchId, CancellationToken cancellationToken);
}

public interface IScoringService
{
    Task<InningsStateResponse?> GetCurrentInningsAsync(Guid matchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<InningsScorecardResponse>> GetScorecardAsync(Guid matchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CommentaryResponse>> GetCommentaryAsync(Guid matchId, CancellationToken cancellationToken);
    Task<InningsStateResponse?> StartInningsAsync(Guid matchId, StartInningsRequest request, CancellationToken cancellationToken);
    Task<InningsStateResponse?> RecordDeliveryAsync(Guid matchId, DeliveryRequest request, CancellationToken cancellationToken);
    Task<InningsStateResponse?> UndoLastDeliveryAsync(Guid matchId, CancellationToken cancellationToken);
    Task<InningsStateResponse?> ChangeBowlerAsync(Guid matchId, ChangeBowlerRequest request, CancellationToken cancellationToken);
    Task<InningsStateResponse?> EndInningsAsync(Guid matchId, CancellationToken cancellationToken);
    Task<bool> EndMatchAsync(Guid matchId, CancellationToken cancellationToken);
}

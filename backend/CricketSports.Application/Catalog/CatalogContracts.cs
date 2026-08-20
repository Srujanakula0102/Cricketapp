using CricketSports.Domain.Enums;

namespace CricketSports.Application.Catalog;

public sealed record TeamRequest(string Name, string ShortName, string CountryOrRegion, string? LogoUrl, bool IsActive = true);
public sealed record TeamResponse(Guid Id, string Name, string ShortName, string CountryOrRegion, string? LogoUrl, bool IsActive);
public sealed record PlayerRequest(string FullName, Guid? TeamId, PlayerRole Role, string? CountryOrRegion, string? ProfileImageUrl, string? BattingStyle, string? BowlingStyle, DateOnly? DateOfBirth);
public sealed record PlayerResponse(Guid Id, string FullName, Guid? TeamId, string? TeamName, PlayerRole Role, string? CountryOrRegion, string? ProfileImageUrl, string? BattingStyle, string? BowlingStyle, DateOnly? DateOfBirth);
public sealed record VenueRequest(string Name, string City, string Country, int? Capacity, string? ImageUrl);
public sealed record VenueResponse(Guid Id, string Name, string City, string Country, int? Capacity, string? ImageUrl);
public sealed record TournamentRequest(string Name, string Season, MatchFormat Format, DateOnly StartDate, DateOnly EndDate, string? LogoUrl, IReadOnlyCollection<Guid>? TeamIds);
public sealed record TournamentResponse(Guid Id, string Name, string Season, MatchFormat Format, DateOnly StartDate, DateOnly EndDate, string? LogoUrl, IReadOnlyCollection<TeamResponse> Teams);
public sealed record MatchRequest(Guid? TournamentId, Guid? VenueId, DateTimeOffset StartsAt, MatchFormat Format, MatchStatus Status, int? OversLimit, string? Notes, Guid HomeTeamId, Guid AwayTeamId);
public sealed record MatchResponse(Guid Id, Guid? TournamentId, string? TournamentName, Guid? VenueId, string? VenueName, DateTimeOffset StartsAt, MatchFormat Format, MatchStatus Status, int? OversLimit, string? Notes, TeamResponse HomeTeam, TeamResponse AwayTeam);
public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalCount, int Page, int PageSize);

public interface ICatalogService
{
    Task<PagedResult<TeamResponse>> GetTeamsAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<TeamResponse?> GetTeamAsync(Guid id, CancellationToken cancellationToken);
    Task<TeamResponse> CreateTeamAsync(TeamRequest request, CancellationToken cancellationToken);
    Task<TeamResponse?> UpdateTeamAsync(Guid id, TeamRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteTeamAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<PlayerResponse>> GetPlayersAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<PlayerResponse?> GetPlayerAsync(Guid id, CancellationToken cancellationToken);
    Task<PlayerResponse?> CreatePlayerAsync(PlayerRequest request, CancellationToken cancellationToken);
    Task<PlayerResponse?> UpdatePlayerAsync(Guid id, PlayerRequest request, CancellationToken cancellationToken);
    Task<bool> DeletePlayerAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<VenueResponse>> GetVenuesAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<VenueResponse?> GetVenueAsync(Guid id, CancellationToken cancellationToken);
    Task<VenueResponse?> CreateVenueAsync(VenueRequest request, CancellationToken cancellationToken);
    Task<VenueResponse?> UpdateVenueAsync(Guid id, VenueRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteVenueAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<TournamentResponse>> GetTournamentsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<TournamentResponse?> GetTournamentAsync(Guid id, CancellationToken cancellationToken);
    Task<TournamentResponse?> CreateTournamentAsync(TournamentRequest request, CancellationToken cancellationToken);
    Task<TournamentResponse?> UpdateTournamentAsync(Guid id, TournamentRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteTournamentAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<MatchResponse>> GetMatchesAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<MatchResponse?> GetMatchAsync(Guid id, CancellationToken cancellationToken);
    Task<MatchResponse?> CreateMatchAsync(MatchRequest request, CancellationToken cancellationToken);
    Task<MatchResponse?> UpdateMatchAsync(Guid id, MatchRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteMatchAsync(Guid id, CancellationToken cancellationToken);
}

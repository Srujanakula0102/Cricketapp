using CricketSports.Application.Catalog;
using CricketSports.Domain.Entities;
using CricketSports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CricketSports.Infrastructure.Catalog;

public sealed class CatalogService(ApplicationDbContext database) : ICatalogService
{
    public async Task<PagedResult<TeamResponse>> GetTeamsAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = database.Teams.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(team => team.Name.Contains(search) || team.CountryOrRegion.Contains(search));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(team => team.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(team => ToResponse(team)).ToListAsync(cancellationToken);
        return new PagedResult<TeamResponse>(items, total, page, pageSize);
    }

    public Task<TeamResponse?> GetTeamAsync(Guid id, CancellationToken cancellationToken) => database.Teams.AsNoTracking().Where(team => team.Id == id).Select(team => ToResponse(team)).SingleOrDefaultAsync(cancellationToken);

    public async Task<TeamResponse> CreateTeamAsync(TeamRequest request, CancellationToken cancellationToken)
    {
        var team = new Team { Name = request.Name.Trim(), ShortName = request.ShortName.Trim().ToUpperInvariant(), CountryOrRegion = request.CountryOrRegion.Trim(), LogoUrl = request.LogoUrl?.Trim(), IsActive = request.IsActive };
        database.Teams.Add(team);
        await database.SaveChangesAsync(cancellationToken);
        return ToResponse(team);
    }

    public async Task<TeamResponse?> UpdateTeamAsync(Guid id, TeamRequest request, CancellationToken cancellationToken)
    {
        var team = await database.Teams.SingleOrDefaultAsync(team => team.Id == id, cancellationToken);
        if (team is null) return null;
        team.Name = request.Name.Trim(); team.ShortName = request.ShortName.Trim().ToUpperInvariant(); team.CountryOrRegion = request.CountryOrRegion.Trim(); team.LogoUrl = request.LogoUrl?.Trim(); team.IsActive = request.IsActive;
        await database.SaveChangesAsync(cancellationToken);
        return ToResponse(team);
    }

    public async Task<bool> DeleteTeamAsync(Guid id, CancellationToken cancellationToken)
    {
        var team = await database.Teams.SingleOrDefaultAsync(team => team.Id == id, cancellationToken);
        if (team is null) return false;
        database.Teams.Remove(team);
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<PlayerResponse>> GetPlayersAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = database.Players.AsNoTracking().Include(player => player.Team).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(player => player.FullName.Contains(search));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(player => player.FullName).Skip((page - 1) * pageSize).Take(pageSize).Select(player => ToResponse(player)).ToListAsync(cancellationToken);
        return new PagedResult<PlayerResponse>(items, total, page, pageSize);
    }

    public Task<PlayerResponse?> GetPlayerAsync(Guid id, CancellationToken cancellationToken) => database.Players.AsNoTracking().Include(player => player.Team).Where(player => player.Id == id).Select(player => ToResponse(player)).SingleOrDefaultAsync(cancellationToken);

    public async Task<PlayerResponse?> CreatePlayerAsync(PlayerRequest request, CancellationToken cancellationToken)
    {
        if (request.TeamId.HasValue && !await database.Teams.AnyAsync(team => team.Id == request.TeamId, cancellationToken)) return null;
        var player = new Player { FullName = request.FullName.Trim(), TeamId = request.TeamId, Role = request.Role, CountryOrRegion = request.CountryOrRegion?.Trim(), ProfileImageUrl = request.ProfileImageUrl?.Trim(), BattingStyle = request.BattingStyle?.Trim(), BowlingStyle = request.BowlingStyle?.Trim(), DateOfBirth = request.DateOfBirth };
        database.Players.Add(player); await database.SaveChangesAsync(cancellationToken);
        await database.Entry(player).Reference(item => item.Team).LoadAsync(cancellationToken); return ToResponse(player);
    }

    public async Task<PlayerResponse?> UpdatePlayerAsync(Guid id, PlayerRequest request, CancellationToken cancellationToken)
    {
        if (request.TeamId.HasValue && !await database.Teams.AnyAsync(team => team.Id == request.TeamId, cancellationToken)) return null;
        var player = await database.Players.Include(item => item.Team).SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (player is null) return null;
        player.FullName = request.FullName.Trim(); player.TeamId = request.TeamId; player.Role = request.Role; player.CountryOrRegion = request.CountryOrRegion?.Trim(); player.ProfileImageUrl = request.ProfileImageUrl?.Trim(); player.BattingStyle = request.BattingStyle?.Trim(); player.BowlingStyle = request.BowlingStyle?.Trim(); player.DateOfBirth = request.DateOfBirth;
        await database.SaveChangesAsync(cancellationToken); await database.Entry(player).Reference(item => item.Team).LoadAsync(cancellationToken); return ToResponse(player);
    }

    public async Task<bool> DeletePlayerAsync(Guid id, CancellationToken cancellationToken) { var player = await database.Players.FindAsync([id], cancellationToken); if (player is null) return false; database.Players.Remove(player); await database.SaveChangesAsync(cancellationToken); return true; }

    public async Task<PagedResult<VenueResponse>> GetVenuesAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100); var query = database.Venues.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(venue => venue.Name.Contains(search) || venue.City.Contains(search));
        var total = await query.CountAsync(cancellationToken); var items = await query.OrderBy(venue => venue.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(venue => ToResponse(venue)).ToListAsync(cancellationToken);
        return new PagedResult<VenueResponse>(items, total, page, pageSize);
    }
    public Task<VenueResponse?> GetVenueAsync(Guid id, CancellationToken cancellationToken) => database.Venues.AsNoTracking().Where(venue => venue.Id == id).Select(venue => ToResponse(venue)).SingleOrDefaultAsync(cancellationToken);
    public async Task<VenueResponse?> CreateVenueAsync(VenueRequest request, CancellationToken cancellationToken) { if (request.Capacity is < 0) return null; var venue = new Venue { Name = request.Name.Trim(), City = request.City.Trim(), Country = request.Country.Trim(), Capacity = request.Capacity, ImageUrl = request.ImageUrl?.Trim() }; database.Venues.Add(venue); await database.SaveChangesAsync(cancellationToken); return ToResponse(venue); }
    public async Task<VenueResponse?> UpdateVenueAsync(Guid id, VenueRequest request, CancellationToken cancellationToken) { if (request.Capacity is < 0) return null; var venue = await database.Venues.FindAsync([id], cancellationToken); if (venue is null) return null; venue.Name = request.Name.Trim(); venue.City = request.City.Trim(); venue.Country = request.Country.Trim(); venue.Capacity = request.Capacity; venue.ImageUrl = request.ImageUrl?.Trim(); await database.SaveChangesAsync(cancellationToken); return ToResponse(venue); }
    public async Task<bool> DeleteVenueAsync(Guid id, CancellationToken cancellationToken) { var venue = await database.Venues.FindAsync([id], cancellationToken); if (venue is null) return false; database.Venues.Remove(venue); await database.SaveChangesAsync(cancellationToken); return true; }

    public async Task<PagedResult<TournamentResponse>> GetTournamentsAsync(int page, int pageSize, CancellationToken ct) { page=Math.Max(1,page); pageSize=Math.Clamp(pageSize,1,100); var q=database.Tournaments.AsNoTracking().Include(x=>x.Teams).ThenInclude(x=>x.Team); var total=await q.CountAsync(ct); var items=(await q.OrderBy(x=>x.StartDate).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct)).Select(ToResponse).ToList(); return new(items,total,page,pageSize); }
    public async Task<TournamentResponse?> GetTournamentAsync(Guid id, CancellationToken ct) { var x=await database.Tournaments.AsNoTracking().Include(x=>x.Teams).ThenInclude(x=>x.Team).SingleOrDefaultAsync(x=>x.Id==id,ct); return x is null?null:ToResponse(x); }
    public async Task<TournamentResponse?> CreateTournamentAsync(TournamentRequest r, CancellationToken ct) { if(r.EndDate<r.StartDate || !await ValidTeams(r.TeamIds,ct)) return null; var x=new Tournament{Name=r.Name.Trim(),Season=r.Season.Trim(),Format=r.Format,StartDate=r.StartDate,EndDate=r.EndDate,LogoUrl=r.LogoUrl?.Trim()}; AddTeams(x,r.TeamIds); database.Tournaments.Add(x); await database.SaveChangesAsync(ct); return ToResponse(x); }
    public async Task<TournamentResponse?> UpdateTournamentAsync(Guid id,TournamentRequest r,CancellationToken ct) { if(r.EndDate<r.StartDate || !await ValidTeams(r.TeamIds,ct)) return null; var x=await database.Tournaments.Include(x=>x.Teams).ThenInclude(x=>x.Team).SingleOrDefaultAsync(x=>x.Id==id,ct); if(x is null)return null; x.Name=r.Name.Trim();x.Season=r.Season.Trim();x.Format=r.Format;x.StartDate=r.StartDate;x.EndDate=r.EndDate;x.LogoUrl=r.LogoUrl?.Trim(); database.TournamentTeams.RemoveRange(x.Teams); x.Teams.Clear();AddTeams(x,r.TeamIds);await database.SaveChangesAsync(ct);return ToResponse(x); }
    public async Task<bool> DeleteTournamentAsync(Guid id,CancellationToken ct){var x=await database.Tournaments.FindAsync([id],ct);if(x is null)return false;database.Tournaments.Remove(x);await database.SaveChangesAsync(ct);return true;}
    public async Task<PagedResult<MatchResponse>> GetMatchesAsync(int page,int pageSize,CancellationToken ct){page=Math.Max(1,page);pageSize=Math.Clamp(pageSize,1,100);var q=MatchQuery();var total=await q.CountAsync(ct);var items=(await q.OrderBy(x=>x.StartsAt).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct)).Select(ToResponse).ToList();return new(items,total,page,pageSize);}
    public async Task<MatchResponse?> GetMatchAsync(Guid id,CancellationToken ct){var x=await MatchQuery().SingleOrDefaultAsync(x=>x.Id==id,ct);return x is null?null:ToResponse(x);}
    public async Task<MatchResponse?> CreateMatchAsync(MatchRequest r,CancellationToken ct){if(!await ValidMatch(r,ct))return null;var x=new Match{TournamentId=r.TournamentId,VenueId=r.VenueId,StartsAt=r.StartsAt,Format=r.Format,Status=r.Status,OversLimit=r.OversLimit,Notes=r.Notes?.Trim()};AddMatchTeams(x,r);database.Matches.Add(x);await database.SaveChangesAsync(ct);return await GetMatchAsync(x.Id,ct);}
    public async Task<MatchResponse?> UpdateMatchAsync(Guid id,MatchRequest r,CancellationToken ct){if(!await ValidMatch(r,ct))return null;var x=await database.Matches.Include(x=>x.Teams).SingleOrDefaultAsync(x=>x.Id==id,ct);if(x is null)return null;x.TournamentId=r.TournamentId;x.VenueId=r.VenueId;x.StartsAt=r.StartsAt;x.Format=r.Format;x.Status=r.Status;x.OversLimit=r.OversLimit;x.Notes=r.Notes?.Trim();database.MatchTeams.RemoveRange(x.Teams);x.Teams.Clear();AddMatchTeams(x,r);await database.SaveChangesAsync(ct);return await GetMatchAsync(id,ct);}
    public async Task<bool> DeleteMatchAsync(Guid id,CancellationToken ct){var x=await database.Matches.FindAsync([id],ct);if(x is null)return false;database.Matches.Remove(x);await database.SaveChangesAsync(ct);return true;}

    private static TeamResponse ToResponse(Team team) => new(team.Id, team.Name, team.ShortName, team.CountryOrRegion, team.LogoUrl, team.IsActive);
    private static PlayerResponse ToResponse(Player player) => new(player.Id, player.FullName, player.TeamId, player.Team?.Name, player.Role, player.CountryOrRegion, player.ProfileImageUrl, player.BattingStyle, player.BowlingStyle, player.DateOfBirth);
    private static VenueResponse ToResponse(Venue venue) => new(venue.Id, venue.Name, venue.City, venue.Country, venue.Capacity, venue.ImageUrl);
    private static TournamentResponse ToResponse(Tournament x)=>new(x.Id,x.Name,x.Season,x.Format,x.StartDate,x.EndDate,x.LogoUrl,x.Teams.Select(y=>ToResponse(y.Team)).ToList());
    private IQueryable<Match> MatchQuery()=>database.Matches.AsNoTracking().Include(x=>x.Tournament).Include(x=>x.Venue).Include(x=>x.Teams).ThenInclude(x=>x.Team);
    private static MatchResponse ToResponse(Match x){var h=x.Teams.Single(y=>y.Role==Domain.Enums.MatchTeamRole.Home).Team;var a=x.Teams.Single(y=>y.Role==Domain.Enums.MatchTeamRole.Away).Team;return new(x.Id,x.TournamentId,x.Tournament?.Name,x.VenueId,x.Venue?.Name,x.StartsAt,x.Format,x.Status,x.OversLimit,x.Notes,ToResponse(h),ToResponse(a));}
    private async Task<bool> ValidTeams(IReadOnlyCollection<Guid>? ids,CancellationToken ct){var set=ids?.Distinct().ToArray()??[];return await database.Teams.CountAsync(x=>set.Contains(x.Id),ct)==set.Length;}
    private static void AddTeams(Tournament x,IReadOnlyCollection<Guid>? ids){foreach(var id in ids?.Distinct()??[])x.Teams.Add(new TournamentTeam{TeamId=id});}
    private async Task<bool> ValidMatch(MatchRequest r,CancellationToken ct)=>r.HomeTeamId!=r.AwayTeamId && (!r.TournamentId.HasValue||await database.Tournaments.AnyAsync(x=>x.Id==r.TournamentId,ct)) && (!r.VenueId.HasValue||await database.Venues.AnyAsync(x=>x.Id==r.VenueId,ct)) && await database.Teams.CountAsync(x=>x.Id==r.HomeTeamId||x.Id==r.AwayTeamId,ct)==2;
    private static void AddMatchTeams(Match x,MatchRequest r){x.Teams.Add(new MatchTeam{TeamId=r.HomeTeamId,Role=Domain.Enums.MatchTeamRole.Home});x.Teams.Add(new MatchTeam{TeamId=r.AwayTeamId,Role=Domain.Enums.MatchTeamRole.Away});}
}

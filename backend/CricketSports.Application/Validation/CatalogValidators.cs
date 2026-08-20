using CricketSports.Application.Catalog;
using CricketSports.Application.Scoring;
using FluentValidation;

namespace CricketSports.Application.Validation;

public sealed class TeamRequestValidator : AbstractValidator<TeamRequest>
{
    public TeamRequestValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(150); RuleFor(x => x.ShortName).NotEmpty().MaximumLength(10); RuleFor(x => x.CountryOrRegion).NotEmpty().MaximumLength(100); RuleFor(x => x.LogoUrl).MaximumLength(500).When(x => x.LogoUrl is not null); }
}
public sealed class PlayerRequestValidator : AbstractValidator<PlayerRequest>
{
    public PlayerRequestValidator() { RuleFor(x => x.FullName).NotEmpty().MaximumLength(150); RuleFor(x => x.BattingStyle).MaximumLength(80).When(x => x.BattingStyle is not null); RuleFor(x => x.BowlingStyle).MaximumLength(80).When(x => x.BowlingStyle is not null); }
}
public sealed class VenueRequestValidator : AbstractValidator<VenueRequest>
{
    public VenueRequestValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(150); RuleFor(x => x.City).NotEmpty().MaximumLength(100); RuleFor(x => x.Country).NotEmpty().MaximumLength(100); RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0).When(x => x.Capacity.HasValue); }
}
public sealed class TournamentRequestValidator : AbstractValidator<TournamentRequest>
{
    public TournamentRequestValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(150); RuleFor(x => x.Season).NotEmpty().MaximumLength(30); RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate); }
}
public sealed class MatchRequestValidator : AbstractValidator<MatchRequest>
{
    public MatchRequestValidator() { RuleFor(x => x.HomeTeamId).NotEqual(x => x.AwayTeamId); RuleFor(x => x.OversLimit).GreaterThan(0).When(x => x.OversLimit.HasValue); RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null); }
}
public sealed class DeliveryRequestValidator : AbstractValidator<DeliveryRequest>
{
    public DeliveryRequestValidator() { RuleFor(x => x.RunsOffBat).GreaterThanOrEqualTo(0); RuleFor(x => x.ExtraRuns).GreaterThanOrEqualTo(0); RuleFor(x => x).Must(x => x.ExtraType == Domain.Enums.ExtraType.None ? x.ExtraRuns == 0 : x.ExtraRuns > 0).WithMessage("Extra runs must agree with the selected extra type."); RuleFor(x => x.IncomingBatterId).NotNull().When(x => x.IsWicket); }
}

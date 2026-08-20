using CricketSports.Application.News;
using FluentValidation;

namespace CricketSports.Application.Validation;

public sealed class NewsArticleRequestValidator : AbstractValidator<NewsArticleRequest>
{
    public NewsArticleRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl is not null);
    }
}

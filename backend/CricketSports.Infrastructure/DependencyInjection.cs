using CricketSports.Infrastructure.Identity;
using CricketSports.Infrastructure.Catalog;
using CricketSports.Infrastructure.Scoring;
using CricketSports.Infrastructure.News;
using CricketSports.Application.Catalog;
using CricketSports.Application.Scoring;
using CricketSports.Application.News;
using CricketSports.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CricketSports.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CricketSportsDatabase")
            ?? throw new InvalidOperationException("Connection string 'CricketSportsDatabase' is not configured.");
        var databaseVersion = configuration["Database:MySqlVersion"] ?? "8.0.36";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(Version.Parse(databaseVersion))));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IScoringService, ScoringService>();
        services.AddScoped<INewsService, NewsService>();

        return services;
    }
}

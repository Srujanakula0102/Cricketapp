using System.Security.Claims;
using CricketSports.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CricketSports.API.Controllers;

public sealed record ManagedUserResponse(string Id, string Email, string DisplayName, IReadOnlyCollection<string> Roles);
public sealed record UpdateUserRolesRequest(IReadOnlyCollection<string> Roles);

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController(UserManager<ApplicationUser> users) : ControllerBase
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase) { "User", "Admin", "Scorer" };

    [HttpGet]
    public async Task<IReadOnlyCollection<ManagedUserResponse>> Get(CancellationToken cancellationToken)
    {
        var result = new List<ManagedUserResponse>();
        foreach (var user in users.Users.OrderBy(item => item.Email))
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(new ManagedUserResponse(user.Id, user.Email ?? string.Empty, user.DisplayName ?? user.Email ?? string.Empty, (await users.GetRolesAsync(user)).ToArray()));
        }
        return result;
    }

    [HttpPut("{id}/roles")]
    public async Task<ActionResult<ManagedUserResponse>> UpdateRoles(string id, UpdateUserRolesRequest request)
    {
        var requested = request.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];
        if (requested.Length == 0 || requested.Any(role => !AllowedRoles.Contains(role))) return BadRequest("Choose one or more valid roles.");
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == currentUserId && !requested.Contains("Admin", StringComparer.OrdinalIgnoreCase)) return BadRequest("You cannot remove your own Admin role.");
        var current = await users.GetRolesAsync(user);
        var remove = await users.RemoveFromRolesAsync(user, current);
        if (!remove.Succeeded) return BadRequest(remove.Errors.Select(error => error.Description));
        var add = await users.AddToRolesAsync(user, requested);
        if (!add.Succeeded) return BadRequest(add.Errors.Select(error => error.Description));
        return Ok(new ManagedUserResponse(user.Id, user.Email ?? string.Empty, user.DisplayName ?? user.Email ?? string.Empty, (await users.GetRolesAsync(user)).ToArray()));
    }
}

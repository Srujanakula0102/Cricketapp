using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CricketSports.API.Configuration;
using CricketSports.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CricketSports.API.Controllers;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, string DisplayName, IReadOnlyCollection<string> Roles);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(UserManager<ApplicationUser> users, IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email, DisplayName = request.DisplayName.Trim() };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors.Select(error => error.Description));
        await users.AddToRoleAsync(user, "User");
        return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !await users.CheckPasswordAsync(user, request.Password)) return Unauthorized();
        var roles = await users.GetRolesAsync(user);
        var options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.ExpiryMinutes);
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, user.Id), new(JwtRegisteredClaimNames.Email, user.Email!), new(ClaimTypes.Name, user.DisplayName ?? user.Email!) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var token = new JwtSecurityToken(options.Issuer, options.Audience, claims, expires: expiresAt.UtcDateTime, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256));
        return Ok(new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, user.DisplayName ?? user.Email!, roles.ToArray()));
    }
}

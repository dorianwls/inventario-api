using System.Security.Claims;
using Inventario.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(UserManager<ApplicationUser> users, TokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized();
        }

        var roles = await users.GetRolesAsync(user);
        return Ok(new LoginResponse(tokens.Create(user, roles), user.DisplayName, roles.ToArray()));
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult Me() => Ok(new
    {
        id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"),
        name = User.Identity?.Name,
        roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value),
    });
}

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string AccessToken, string DisplayName, IReadOnlyCollection<string> Roles);

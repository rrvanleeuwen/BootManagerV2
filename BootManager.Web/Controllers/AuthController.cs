using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BootManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IOwnerLoginService _login;
    private readonly IConfiguration _config;

    public AuthController(IOwnerLoginService login, IConfiguration config)
    {
        _login = login;
        _config = config;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] LoginRequestDto req, CancellationToken ct)
    {
        var result = await _login.ValidateAsync(req, ct);
        if (!result.Success || result.OwnerId is null)
            return BadRequest(new { message = result.Message ?? "Inloggen mislukt." });

        var key = _config["Jwt:Key"] ?? "please_change_this_secret_for_production";
        var issuer = _config["Jwt:Issuer"] ?? "bootmanager";
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.OwnerId.Value.ToString()),
            new(ClaimTypes.Name, "Owner"),
            new(ClaimTypes.Role, "Owner")
        };

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { access_token = tokenStr, token_type = "Bearer", expires_in = expiryMinutes * 60 });
    }
}

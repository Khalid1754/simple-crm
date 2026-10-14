using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleCrm.Api.Data;
using SimpleCrm.Api.Dtos;
using SimpleCrm.Api.Models;
using SimpleCrm.Api.Services;

namespace SimpleCrm.Api.Controllers;

/// <summary>
/// Registrering och inloggning. Dessa endpoints är öppna (ingen [Authorize]).
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly CrmDbContext _db;
    private readonly TokenService _tokenService;
    // PasswordHasher kommer från ASP.NET Identity och hashar lösenord säkert.
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(CrmDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict("Det finns redan en användare med den e-postadressen.");
        }

        var user = new User { Email = email };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return Ok(new AuthResponseDto(token, user.Email, expiresAt));
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Samma felmeddelande oavsett om e-post eller lösenord är fel,
        // så att man inte kan lista ut vilka e-postadresser som finns.
        if (user is null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Fel e-post eller lösenord.");
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return Ok(new AuthResponseDto(token, user.Email, expiresAt));
    }
}

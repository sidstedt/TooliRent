using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TooliRent.Application.DTOs;
using TooliRent.Domain.Entities;
using TooliRent.Infrastructure.Persistence;
using TooliRent.WebApi.Auth;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _user;
        private readonly IJwtTokenService _jwtToken;
        private readonly RoleManager<IdentityRole<Guid>> _role;
        private readonly TooliRentDbContext _db;

        public AuthController(
            UserManager<ApplicationUser> user,
            RoleManager<IdentityRole<Guid>> role,
            IJwtTokenService jwtToken,
            TooliRentDbContext db)
        {
            _user = user;
            _role = role;
            _jwtToken = jwtToken;
            _db = db;
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            var user = await _user.FindByEmailAsync(dto.Email);
            if (user is null || !await _user.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }
            if (user.IsActive is false)
            {
                return Unauthorized(new { message = "User account is inactive." });
            }

            var roles = await _user.GetRolesAsync(user);
            var accessToken = _jwtToken.CreateToken(user, roles);

            var genRefToken = GenerateSecureToken();
            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = Hash(genRefToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync(ct);

            return Ok(new { token = accessToken, refreshToken = genRefToken });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto, CancellationToken ct)
        {
            var plain = dto?.RefreshToken;
            if (string.IsNullOrWhiteSpace(plain))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }
            var now = DateTime.UtcNow;
            var hash = Hash(plain);

            var token = await _db.RefreshTokens
                .AsTracking()
                .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

            if (token is null || token.ExpiresAt <= now || token.UsedAt is not null || token.RevokedAt is not null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var user = await _user.FindByIdAsync(token.UserId.ToString());
            if (user is null)
            {
                return Unauthorized(new { message = "User not found." });
            }

            // Invalidate current token
            token.UsedAt = now;

            // Issue new tokens
            var roles = await _user.GetRolesAsync(user);
            var accessToken = _jwtToken.CreateToken(user, roles);
            
            var nextPlain = GenerateSecureToken();
            var next = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = Hash(nextPlain),
                ExpiresAt = now.AddDays(7)
            };
            _db.RefreshTokens.Add(next);

            await _db.SaveChangesAsync(ct);

            return Ok(new { token = accessToken, refreshToken = nextPlain });
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto? dto, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var plain = dto?.RefreshToken;
            if (!string.IsNullOrWhiteSpace(plain))
            {
                var hash = Hash(plain);
                var rt = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
                if (rt != null && rt.RevokedAt == null)
                {
                    rt.RevokedAt = now;
                    await _db.SaveChangesAsync(ct);
                }
            }

            return Ok(new { message = "Logged out." });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            var existingUser = await _user.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
            {
                return BadRequest(new { message = "Email is already in use." });
            }
            var newUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName ?? string.Empty,
                LastName = dto.LastName ?? string.Empty,
                DisplayName = dto.DisplayName ?? string.Empty
            };
            var result = await _user.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            
            var defaultRole = "Member";
            if (!await _role.RoleExistsAsync(defaultRole))
            {
                await _role.CreateAsync(new IdentityRole<Guid>(defaultRole));
            }
            await _user.AddToRoleAsync(newUser, defaultRole);

            return Ok(new { message = "User registered successfully." });
        }
    }
}

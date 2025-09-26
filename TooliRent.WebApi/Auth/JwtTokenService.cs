using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TooliRent.Domain.Entities;

namespace TooliRent.WebApi.Auth
{
    public interface IJwtTokenService
    {
        string CreateToken(ApplicationUser user, IEnumerable<string>? roles = null);
    }
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _cfg;

        public JwtTokenService(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        public string CreateToken(ApplicationUser user, IEnumerable<string>? roles = null)
        {
            var jwt = _cfg.GetSection("Jwt");
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new (ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
            };
            if (roles is not null)
            {
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

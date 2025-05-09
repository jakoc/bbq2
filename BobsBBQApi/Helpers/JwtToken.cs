using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BobsBBQApi.Helpers.Interfaces;
using BobsBBQApi.Services;
using Microsoft.IdentityModel.Tokens;

namespace BobsBBQApi.Helpers;

public class JwtToken : IJwtToken
{
    public string GenerateJwtToken(Guid userId, string email, string role)
    {
        MonitorService.Log.Information("Generating JWT token for userId: {@userId}, email: {@email}, role: {@role}",
            userId, email, role);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("your_new_32_byte_or_longer_key_here_12345");
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("email", email), 
                new Claim("role", role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        try
        {
            MonitorService.Log.Information("Attempting to create JWT token for userId: {@userId}", userId);

            var token = tokenHandler.CreateToken(tokenDescription);

            MonitorService.Log.Information("JWT token successfully created for userId: {@userId}", userId);

            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error occurred while generating JWT token for userId: {@userId}", userId);
            throw;
        }
    }
}
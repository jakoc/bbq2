using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BobsBBQApi.Helpers.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BobsBBQApi.Helpers;

public class JwtToken : IJwtToken
{
    public string GenerateJwtToken(string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("your_new_32_byte_or_longer_key_here_12345");
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("email", email), 
                new Claim("role", role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescription);
        return tokenHandler.WriteToken(token);
    }
}
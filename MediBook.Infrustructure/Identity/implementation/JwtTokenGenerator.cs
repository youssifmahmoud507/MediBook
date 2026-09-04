using MediBook.Infrustructure.Identity.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MediBook.Infrustructure.Identity.implementation
{
    public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration = configuration;
        public string GenerateToken(ApplicationUser user, IList<string> roles)
        {
            /*Build a List<Claim>: new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email!), plus one new Claim(ClaimTypes.Role, role) for each role in roles.*/
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            /*Create signing credentials from the Jwt:Key using SymmetricSecurityKey + SigningCredentials (HMAC-SHA256 is standard: SecurityAlgorithms.HmacSha256).*/
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            /*Construct a JwtSecurityToken with issuer, audience, claims, expiry (DateTime.UtcNow.AddMinutes(expiryMinutes)), and the signing credentials.*/
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

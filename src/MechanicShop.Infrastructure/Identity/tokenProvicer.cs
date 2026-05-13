using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MechanicShop.Infrastructure.Identity;

public class TokenProvider(
    IConfiguration configuration,
    IAppDbContext context
) : ITokenProvider
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IAppDbContext _context = context;

    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken cancellationToken = default)
    {
        var tokenResult = await CreateAsync(user, cancellationToken);

        if(tokenResult.IsError)
            return tokenResult.Errors;

        return tokenResult.Value;    
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!)),
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if(securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token.");
        }

        return principal;
    }

    private async Task<Result<TokenResponse>> CreateAsync(AppUserDto user, CancellationToken cancellationToken)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"]!));

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.UserId!),
            new (JwtRegisteredClaimNames.Email, user.Email!)
        };

        foreach(var role in user.Roles)
        {
            claims.Add(new (ClaimTypes.Role, role));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key,
                                 SecurityAlgorithms.HmacSha256Signature)
        };
         
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenObject = tokenHandler.CreateToken(descriptor);

        var oldRefreshTokens = await _context.RefreshTokens
            .Where(rToken => rToken.UserId == user.UserId)
            .ExecuteDeleteAsync(cancellationToken); 

        var newRefreshTokenResult = RefreshToken.Create(
            Guid.NewGuid(),
            GenerateRefreshToken(),
            user.UserId,
            DateTime.UtcNow.AddDays(7)
        ); 

        if (newRefreshTokenResult.IsError)
        {
            return newRefreshTokenResult.Errors;
        }

        var refreshToken = newRefreshTokenResult.Value;

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(tokenObject),
            RefreshToken = refreshToken.Token,
            ExpiresAtUtc = expires
        };
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
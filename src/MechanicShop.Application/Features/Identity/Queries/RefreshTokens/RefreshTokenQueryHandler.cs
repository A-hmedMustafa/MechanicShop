using System.Security.Claims;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

public class RefreshTokenQueryHandler(
    ILogger<RefreshTokenQueryHandler> logger,
    IIdentityService identityService,
    IAppDbContext context,
    ITokenProvider tokenProvider
) : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly IAppDbContext _context = context;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var principal = _tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

        if(principal is null)
        {
            _logger.LogError("Expired access token is not valid");
            
            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if(userId is null)
        {
            _logger.LogError("Invalid userId claim");

            return ApplicationErrors.UserIdClaimInvalid;
        }

        var getUserResult = await _identityService.GetUserByIdAsync(userId);

        if (getUserResult.IsError)
        {
            _logger.LogError("Get user by id error occurred: {ErrorDescription}", getUserResult.TopError.Description);
            
            return getUserResult.Errors;
        }

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == request.RefreshToken
            && refreshToken.UserId == userId, cancellationToken);

        var tokenGenerationResult = await _tokenProvider.GenerateJwtTokenAsync(getUserResult.Value);

        if (tokenGenerationResult.IsError)
        {
            _logger.LogError("Generate token error occurred: {ErrorDescription}", tokenGenerationResult.TopError.Description);

            return tokenGenerationResult.Errors;
        }
        
        return tokenGenerationResult.Value;
    }
}
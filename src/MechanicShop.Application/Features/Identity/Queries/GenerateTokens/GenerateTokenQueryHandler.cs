using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

public class GenerateTokenQueryHandler(
    ILogger<GenerateTokenQueryHandler> logger, 
    IIdentityService identityService,
    ITokenProvider tokenProvider
) : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<GenerateTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    public async Task<Result<TokenResponse>> Handle(GenerateTokenQuery request, CancellationToken cancellationToken)
    {
        var userResponse = await _identityService.AuthenticateAsync(request.Email, request.Password);
        
        if(userResponse.IsError)
            return userResponse.Errors;

        var tokenGenerationResult = await _tokenProvider.GenerateJwtTokenAsync(userResponse.Value,cancellationToken);

        if(tokenGenerationResult.IsError)
        {
            _logger.LogError("Token Generation Failed : {ErrorDescription}", tokenGenerationResult.TopError.Description);

            return tokenGenerationResult.Errors;
        }   

        return tokenGenerationResult.Value;
    }
}
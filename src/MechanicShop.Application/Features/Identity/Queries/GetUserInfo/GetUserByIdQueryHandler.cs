using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public class GetUserByIdQueryHandler(ILogger<GetUserByIdQueryHandler> logger, IIdentityService identityService) : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
{
    private readonly ILogger<GetUserByIdQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var getUserResult = await _identityService.GetUserByIdAsync(request.UserId!);

        if (getUserResult.IsError)
        {
            _logger.LogError("User With Id {UserId}{ErrorDetails}",request.UserId, getUserResult.TopError.Description);

            return getUserResult.Errors;
        }

        return getUserResult.Value;
    }
}
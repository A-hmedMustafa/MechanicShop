using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskCommandHandler(
    ILogger<UpdateRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
    ) : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
    private readonly ILogger<UpdateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks
            .Include(repairTask => repairTask.Parts)
            .FirstOrDefaultAsync(repairTask => repairTask.Id == request.RepairTaskId,
            cancellationToken);

        if(repairTask is null)
        {
            _logger.LogWarning("RepairTask {RepairTaskId} not found for update.", request.RepairTaskId);

            return ApplicationErrors.RepairTaskNotFound;
        }    

        var validatedParts = new List<Part>();

        foreach(var part in request.Parts)
        {
            var partId = part.PartId ?? Guid.NewGuid();

            var partCreationResult = Part.Create(partId, part.Name, part.Cost, part.Quantity);

            if(partCreationResult.IsError)
            {
                return partCreationResult.Errors;  
            }

            validatedParts.Add(partCreationResult.Value);
        }

        var repairTaskUpdateResult = repairTask.Update(request.Name,request.LaborCost,request.EstimatedDurationInMins);

        if (repairTaskUpdateResult.IsError)
        {
            return repairTaskUpdateResult.Errors;
        }

        var upsertPartsResult = repairTask.UpsertParts(validatedParts);

        if (upsertPartsResult.IsError)
        {
            return upsertPartsResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("repair-task",cancellationToken);

        return Result.Updated;
    }
}
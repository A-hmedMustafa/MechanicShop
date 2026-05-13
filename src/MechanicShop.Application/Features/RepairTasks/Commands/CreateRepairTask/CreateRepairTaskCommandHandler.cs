using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskCommandHandler(
    ILogger<CreateRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
) : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
{
    private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _context.RepairTasks
            .AnyAsync(repairTask => EF.Functions.Like(repairTask.Name, request.Name), cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Duplicates Part Name '{PartName}'.", request.Name);
            
            return RepairTaskErrors.DuplicateName;
        }    

        List<Part> parts = [];

        foreach(var part in request.Parts)
        {
            var partCreationResult = Part.Create(Guid.NewGuid(), part.Name, part.Cost, part.Quantity);

            if (partCreationResult.IsError)
            {
                return partCreationResult.Errors;
            }

            parts.Add(partCreationResult.Value);
        }

        var repairTaskCreationResult = RepairTask.Create(
            id: Guid.NewGuid(),
            name: request.Name!,
            laborCost: request.LaborCost,
            estimatedDurationInMins: request.EstimatedDurationInMins!.Value,
            parts: parts
        );

        if (repairTaskCreationResult.IsError)
        {
            return repairTaskCreationResult.Errors;
        }

        var repairTask = repairTaskCreationResult.Value;

        _context.RepairTasks.Add(repairTask);

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("repair-task",cancellationToken);

        return repairTask.ToDto();
    }
}
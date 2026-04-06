using InsuraTech.Application.HealthPlans.DTOs;
using InsuraTech.Application.HealthPlans.Queries.CalculateHealthPlan;
using InsuraTech.Application.HealthPlans.Queries.GetHealthPlans;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InsuraTech.API.Controllers;

[ApiController]
[Route("api/v1/health-plans")]
[Produces("application/json")]
public sealed class HealthPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public HealthPlansController(IMediator mediator) => _mediator = mediator;

    /// <summary>Obtiene el catálogo de planes de salud disponibles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HealthPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetHealthPlansQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Calcula el monto asegurado para un plan y fecha de nacimiento dados.</summary>
    [HttpGet("calculate")]
    [ProducesResponseType(typeof(HealthPlanCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Calculate(
        [FromQuery] string planId,
        [FromQuery] DateOnly birthDate,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CalculateHealthPlanQuery { PlanId = planId, BirthDate = birthDate },
            cancellationToken);

        return Ok(result);
    }
}

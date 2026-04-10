using InsuraTech.Application.LifePlans.DTOs;
using InsuraTech.Application.LifePlans.Queries.CalculateLifePlan;
using InsuraTech.Application.LifePlans.Queries.GetLifePlans;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InsuraTech.API.Controllers;

[ApiController]
[Route("api/v1/life-plans")]
[Produces("application/json")]
public sealed class LifePlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public LifePlansController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lista todos los planes de vida disponibles con sus beneficios.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LifePlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLifePlansQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Valida edad del asegurado y retorna la cotización del plan de vida seleccionado.</summary>
    [HttpGet("calculate")]
    [ProducesResponseType(typeof(LifePlanCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Calculate(
        [FromQuery] string planId,
        [FromQuery] DateOnly birthDate,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CalculateLifePlanQuery { PlanId = planId, BirthDate = birthDate },
            cancellationToken);

        return Ok(result);
    }
}

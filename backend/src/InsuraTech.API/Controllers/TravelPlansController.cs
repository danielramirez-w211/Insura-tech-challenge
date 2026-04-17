using InsuraTech.Application.TravelPlans.DTOs;
using InsuraTech.Application.TravelPlans.Queries.CalculateTravelPlan;
using InsuraTech.Domain.Policies.TravelPlan;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuraTech.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/travel-plans")]
[Produces("application/json")]
public sealed class TravelPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public TravelPlansController(IMediator mediator) => _mediator = mediator;

    /// <summary>Calcula el valor de una póliza de viaje dado el tipo, continente y duración.</summary>
    [HttpGet("calculate")]
    [ProducesResponseType(typeof(TravelPlanCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Calculate(
        [FromQuery] TripType tripType,
        [FromQuery] Continent? continent,
        [FromQuery] int durationDays,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CalculateTravelPlanQuery
            {
                TripType    = tripType,
                Continent   = continent,
                DurationDays = durationDays
            },
            cancellationToken);

        return Ok(result);
    }
}

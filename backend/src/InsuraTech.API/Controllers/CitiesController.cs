using InsuraTech.Application.Cities.DTOs;
using InsuraTech.Application.Cities.Queries.GetCities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuraTech.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class CitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CitiesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns the list of available Colombian cities.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCitiesQuery(), cancellationToken);
        return Ok(result);
    }
}

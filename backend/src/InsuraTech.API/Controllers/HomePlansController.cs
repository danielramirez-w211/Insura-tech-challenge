using InsuraTech.Application.HomePlans.DTOs;
using InsuraTech.Application.HomePlans.Queries.CalculateHomeQuotation;
using InsuraTech.Application.HomePlans.Queries.GetHomePlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuraTech.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/home-plans")]
[Produces("application/json")]
public sealed class HomePlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public HomePlansController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lista los tres paquetes de cobertura de hogar con sus coberturas incluidas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HomePlanPackageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetHomePlansQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Ejecuta el motor de cotización de hogar y retorna la prima mensual calculada.
    /// Se usa POST porque el payload incluye una lista variable de coberturas seleccionadas.
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(HomeQuotationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Calculate(
        [FromBody] CalculateHomeQuotationQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}

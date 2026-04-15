using InsuraTech.Application.VehiclePlans.DTOs;
using InsuraTech.Application.VehiclePlans.Queries.CalculateVehiclePlans;
using InsuraTech.Application.VehiclePlans.Queries.GetVehiclePlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuraTech.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/vehicle-plans")]
[Produces("application/json")]
public sealed class VehiclePlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclePlansController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lista los tres planes de cobertura vehicular con sus multiplicadores y asistencias.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VehiclePlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVehiclePlansQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Ejecuta el motor de cotización vehicular y retorna las primas de los tres planes.
    /// </summary>
    /// <param name="commercialValue">Valor comercial del vehículo (COP). Debe ser mayor a 0.</param>
    /// <param name="vehicleYear">Año de fabricación del vehículo.</param>
    /// <param name="brand">Marca del vehículo.</param>
    [HttpGet("calculate")]
    [ProducesResponseType(typeof(VehicleQuotationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Calculate(
        [FromQuery] decimal commercialValue,
        [FromQuery] int     vehicleYear,
        [FromQuery] string  brand,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CalculateVehiclePlansQuery
            {
                CommercialValue = commercialValue,
                VehicleYear     = vehicleYear,
                Brand           = brand
            },
            cancellationToken);

        return Ok(result);
    }
}

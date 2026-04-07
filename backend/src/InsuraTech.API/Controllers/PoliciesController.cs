namespace InsuraTech.API.Controllers;

using InsuraTech.API.Models;
using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Application.Claims.Queries.GetClaimsByPolicy;
using InsuraTech.Application.Policies.Commands.ActivatePolicy;
using InsuraTech.Application.Policies.Commands.CancelPolicy;
using InsuraTech.Application.Policies.Commands.CreatePolicy;
using InsuraTech.Application.Policies.Commands.RenewPolicy;
using InsuraTech.Application.Policies.Commands.SuspendPolicy;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Application.Policies.Queries.GetPolicies;
using InsuraTech.Application.Policies.Queries.GetPolicyById;
using InsuraTech.Application.Common.Models;
using InsuraTech.Domain.Policies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class PoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PoliciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new insurance policy.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePolicyRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var command = new CreatePolicyCommand
        {
            IdempotencyKey      = idempotencyKey ?? Guid.NewGuid().ToString(),
            Type                = request.Type,
            InsuredFirstName    = request.Insured.FirstName,
            InsuredLastName     = request.Insured.LastName,
            InsuredDocumentType = request.Insured.DocumentType,
            InsuredDocumentId   = request.Insured.DocumentId,
            InsuredBirthDate    = request.Insured.BirthDate,
            CoverageStartDate   = request.CoveragePeriod.StartDate,
            CoverageEndDate     = request.CoveragePeriod.EndDate,
            InsuredAmount       = request.InsuredAmount,
            MonthlyPremium      = request.MonthlyPremium,
            HealthPlanId        = request.HealthPlanId,
            TripType            = request.TripType,
            Continent           = request.Continent,
            DurationDays        = request.DurationDays,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Gets all policies with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PolicyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PolicyStatus? status,
        [FromQuery] PolicyType? type,
        [FromQuery] string? documentId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPoliciesQuery
        {
            Status = status,
            Type = type,
            DocumentId = documentId,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a policy by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPolicyByIdQuery { PolicyId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Activates a pending policy.</summary>
    [HttpPut("{id:guid}/activate")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ActivatePolicyCommand { PolicyId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Suspends an active policy.</summary>
    [HttpPut("{id:guid}/suspend")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Suspend(
        Guid id,
        [FromBody] SuspendPolicyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { PolicyId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cancels a policy.</summary>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelPolicyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { PolicyId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets all claims for a policy.</summary>
    [HttpGet("{id:guid}/claims")]
    [ProducesResponseType(typeof(IEnumerable<ClaimResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaims(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClaimsByPolicyQuery { PolicyId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Renews a policy generating a new one.</summary>
    [HttpPost("{id:guid}/renew")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Renew(
        Guid id,
        [FromBody] RenewPolicyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { PolicyId = id }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
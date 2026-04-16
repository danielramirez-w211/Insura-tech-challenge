namespace InsuraTech.API.Controllers;

using System.Security.Claims;
using InsuraTech.Application.Claims.Commands.AppealClaim;
using InsuraTech.Application.Claims.Commands.ApproveClaim;
using InsuraTech.Application.Claims.Commands.ApprovePendingClaim;
using InsuraTech.Application.Claims.Commands.RegisterClaim;
using InsuraTech.Application.Claims.Commands.RegisterPayment;
using InsuraTech.Application.Claims.Commands.RejectClaim;
using InsuraTech.Application.Claims.Commands.RejectPendingClaim;
using InsuraTech.Application.Claims.Commands.StartInvestigation;
using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Application.Claims.Queries.GetClaimById;
using InsuraTech.Application.Claims.Queries.GetClaims;
using InsuraTech.Application.Common.Models;
using InsuraTech.Domain.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class ClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator) => _mediator = mediator;

    private string CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email)
                                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private Guid   CurrentUserId    => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Gets all claims with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClaimResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ClaimStatus? status,
        [FromQuery] Guid? policyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetClaimsQuery { Status = status, PolicyId = policyId, Page = page, PageSize = pageSize },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Registers a new claim — starts in PendingApproval for Leader review.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterClaimCommand body,
        CancellationToken cancellationToken)
    {
        var command = body with
        {
            CreatedByAdvisorId = User.FindFirstValue(ClaimTypes.Role) == "Advisor"
                ? CurrentUserId
                : null,
        };
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Gets a claim by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClaimByIdQuery { ClaimId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Starts investigation of a claim.</summary>
    [HttpPut("{id:guid}/investigate")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Investigate(
        Guid id,
        [FromBody] StartInvestigationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { ClaimId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Approves a claim.</summary>
    [HttpPut("{id:guid}/approve")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveClaimCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { ClaimId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Rejects a claim.</summary>
    [HttpPut("{id:guid}/reject")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectClaimCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { ClaimId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Appeals a rejected claim.</summary>
    [HttpPut("{id:guid}/appeal")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Appeal(
        Guid id,
        [FromBody] AppealClaimCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { ClaimId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Registers payment of an approved claim.</summary>
    [HttpPut("{id:guid}/pay")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(
        Guid id,
        [FromBody] RegisterPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { ClaimId = id }, cancellationToken);
        return Ok(result);
    }

    // ── Flujo de aprobación del líder (SPEC-014) ──────────────────────────────

    /// <summary>El Líder aprueba un siniestro en estado PendingApproval — pasa a Registered.</summary>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Leader")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApprovePending(Guid id, CancellationToken cancellationToken)
    {
        var command = new ApprovePendingClaimCommand
        {
            ClaimId         = id,
            ResponsibleUser = CurrentUserEmail,
        };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>El Líder rechaza un siniestro en estado PendingApproval con motivo.</summary>
    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = "Leader")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectPending(
        Guid id,
        [FromBody] RejectPendingRequest body,
        CancellationToken cancellationToken)
    {
        var command = new RejectPendingClaimCommand
        {
            ClaimId         = id,
            Reason          = body.Reason,
            ResponsibleUser = CurrentUserEmail,
        };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

/// <summary>Body para PATCH /{id}/reject.</summary>
public sealed record RejectPendingRequest(string Reason);
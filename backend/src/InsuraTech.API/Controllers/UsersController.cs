namespace InsuraTech.API.Controllers;

using System.Security.Claims;
using InsuraTech.Application.Users.Commands.CreateAdvisor;
using InsuraTech.Application.Users.Commands.CreateLeader;
using InsuraTech.Application.Users.Commands.UpdateProfile;
using InsuraTech.Application.Users.Commands.UpdateUserStatus;
using InsuraTech.Application.Users.DTOs;
using InsuraTech.Application.Users.Queries.GetLeaders;
using InsuraTech.Application.Users.Queries.GetMyAdvisors;
using InsuraTech.Application.Users.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    // ── Helpers JWT ──────────────────────────────────────────────────────────

    private Guid   CurrentUserId   => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole     => User.FindFirstValue(ClaimTypes.Role)!;
    private string CurrentEmail    => User.FindFirstValue(ClaimTypes.Email)!;
    private Guid?  CurrentLeaderId =>
        Guid.TryParse(User.FindFirstValue("leaderId"), out var id) && id != Guid.Empty ? id : null;

    // ── GET /me ──────────────────────────────────────────────────────────────

    /// <summary>Retorna el perfil completo del usuario autenticado.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyProfileQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    // ── PUT /me/profile ──────────────────────────────────────────────────────

    /// <summary>Actualiza el perfil del usuario autenticado.</summary>
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateProfileCommand body,
        CancellationToken cancellationToken)
    {
        var command = body with { UserId = CurrentUserId };
        var result  = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── POST /leaders ─────────────────────────────────────────────────────────

    /// <summary>Crea un nuevo Líder. Solo Admin.</summary>
    [HttpPost("leaders")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateLeader(
        [FromBody] CreateLeaderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMe), result);
    }

    // ── GET /leaders ──────────────────────────────────────────────────────────

    /// <summary>Lista todos los Líderes. Solo Admin.</summary>
    [HttpGet("leaders")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLeadersQuery(), cancellationToken);
        return Ok(result);
    }

    // ── POST /advisors ────────────────────────────────────────────────────────

    /// <summary>Crea un nuevo Asesor. Solo Leader.</summary>
    [HttpPost("advisors")]
    [Authorize(Roles = "Leader")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAdvisor(
        [FromBody] CreateAdvisorCommand body,
        CancellationToken cancellationToken)
    {
        var command = body with { LeaderId = CurrentUserId };
        var result  = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMe), result);
    }

    // ── GET /my-advisors ──────────────────────────────────────────────────────

    /// <summary>Lista los asesores del líder autenticado con salesCount. Solo Leader.</summary>
    [HttpGet("my-advisors")]
    [Authorize(Roles = "Leader")]
    [ProducesResponseType(typeof(IReadOnlyList<AdvisorSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAdvisors(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyAdvisorsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    // ── PATCH /{id}/status ────────────────────────────────────────────────────

    /// <summary>Activa o desactiva un usuario. Admin gestiona Líderes; Leader gestiona sus Asesores.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Leader")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateUserStatusRequest body,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserStatusCommand
        {
            TargetUserId      = id,
            IsActive          = body.IsActive,
            RequesterId       = CurrentUserId,
            RequesterRole     = CurrentRole,
            RequesterLeaderId = CurrentLeaderId,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

/// <summary>Body para PATCH /{id}/status.</summary>
public sealed record UpdateUserStatusRequest(bool IsActive);

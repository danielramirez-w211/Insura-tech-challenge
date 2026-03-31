namespace InsuraTech.API.Controllers;

using InsuraTech.Application.Common.Models;
using InsuraTech.Application.Notifications.Commands.RetryNotification;
using InsuraTech.Application.Notifications.DTOs;
using InsuraTech.Application.Notifications.Queries.GetNotifications;
using InsuraTech.Domain.Notifications;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lists notifications with optional status filter.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] NotificationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetNotificationsQuery { Status = status, Page = page, PageSize = pageSize },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Retries a failed notification.</summary>
    [HttpPut("{id:guid}/retry")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Retry(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RetryNotificationCommand(id), cancellationToken);
        return Ok(result);
    }
}

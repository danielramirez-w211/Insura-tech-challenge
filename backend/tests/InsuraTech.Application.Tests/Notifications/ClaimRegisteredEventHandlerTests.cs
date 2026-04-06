namespace InsuraTech.Application.Tests.Notifications;

using FluentAssertions;
using InsuraTech.Application.Notifications.EventHandlers;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using NSubstitute;

public sealed class ClaimRegisteredEventHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private ClaimRegisteredEventHandler Sut => new(_notificationRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenClaimRegisteredEvent_ShouldCreateAndStoreNotification()
    {
        // GIVEN
        var @event = new ClaimRegisteredEvent(Guid.NewGuid(), Guid.NewGuid(), "POL-2024-00000001", 5_000m);

        // WHEN
        await Sut.Handle(@event, CancellationToken.None);

        // THEN
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.Type == NotificationType.ClaimRegistered &&
                n.Subject == "Claim Registered"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClaimRegisteredEvent_ShouldSaveChanges()
    {
        // GIVEN
        var @event = new ClaimRegisteredEvent(Guid.NewGuid(), Guid.NewGuid(), "POL-2024-00000001", 5_000m);

        // WHEN
        await Sut.Handle(@event, CancellationToken.None);

        // THEN
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClaimRegisteredEvent_ShouldIncludeClaimAmountInBody()
    {
        // GIVEN
        var claimId = Guid.NewGuid();
        var policyId = Guid.NewGuid();
        var @event = new ClaimRegisteredEvent(claimId, policyId, "POL-2024-00000001", 5_000m);

        // WHEN
        await Sut.Handle(@event, CancellationToken.None);

        // THEN
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.Body.Contains("5,000.00") &&
                n.Body.Contains(claimId.ToString())),
            Arg.Any<CancellationToken>());
    }
}

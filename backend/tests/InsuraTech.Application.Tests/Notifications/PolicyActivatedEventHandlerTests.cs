namespace InsuraTech.Application.Tests.Notifications;

using FluentAssertions;
using InsuraTech.Application.Notifications.EventHandlers;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using NSubstitute;

public sealed class PolicyActivatedEventHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private PolicyActivatedEventHandler Sut => new(_notificationRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenPolicyActivatedEvent_ShouldCreateAndStoreNotification()
    {
        // GIVEN
        var @event = new PolicyActivatedEvent(
            Guid.NewGuid(), "POL-2024-00000001", "Ana Lopez", "12345678");

        // WHEN
        await Sut.Handle(@event, CancellationToken.None);

        // THEN
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.Type == NotificationType.PolicyActivated &&
                n.Subject == "Policy Activated"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPolicyActivatedEvent_ShouldSaveChanges()
    {
        // GIVEN
        var @event = new PolicyActivatedEvent(
            Guid.NewGuid(), "POL-2024-00000001", "Ana Lopez", "12345678");

        // WHEN
        await Sut.Handle(@event, CancellationToken.None);

        // THEN
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPolicyActivatedEvent_ShouldIncludeInsuredInfoInBody()
    {
        // GIVEN
        var policyId = Guid.NewGuid();
        var @event = new PolicyActivatedEvent(policyId, "POL-2024-00000001", "Ana Lopez", "12345678");

        // WHEN
        await Sut.Handle(@event, CancellationToken.None);

        // THEN
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.Body.Contains("Ana Lopez") &&
                n.Body.Contains("12345678")),
            Arg.Any<CancellationToken>());
    }
}

namespace InsuraTech.Application.Tests.Notifications;

using FluentAssertions;
using InsuraTech.Application.Notifications.Commands.RetryNotification;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using NSubstitute;

public sealed class RetryNotificationHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private RetryNotificationHandler Sut => new(_notificationRepository, _unitOfWork);

    private static Notification BuildFailedNotification()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), "Test User", "Subject", "Body", NotificationType.PolicyActivated);
        notification.MarkAsFailed("SMTP error");
        return notification;
    }

    [Fact]
    public async Task Handle_WhenNotificationIsFailed_ShouldRetryAndReturnDto()
    {
        // GIVEN
        var notification = BuildFailedNotification();
        _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        // WHEN
        var result = await Sut.Handle(new RetryNotificationCommand(notification.Id), CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Status.Should().Be(NotificationStatus.Pending);
    }

    [Fact]
    public async Task Handle_WhenNotificationIsFailed_ShouldCallUpdateAndSave()
    {
        // GIVEN
        var notification = BuildFailedNotification();
        _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        // WHEN
        await Sut.Handle(new RetryNotificationCommand(notification.Id), CancellationToken.None);

        // THEN
        await _notificationRepository.Received(1).UpdateAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotificationNotFound_ShouldThrowNotFoundException()
    {
        // GIVEN
        _notificationRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        // WHEN
        var act = async () => await Sut.Handle(new RetryNotificationCommand(Guid.NewGuid()), CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotificationIsNotFailed_ShouldThrowInvalidOperationException()
    {
        // GIVEN
        var notification = Notification.Create(
            Guid.NewGuid(), "Test User", "Subject", "Body", NotificationType.PolicyActivated);
        _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        // WHEN
        var act = async () => await Sut.Handle(new RetryNotificationCommand(notification.Id), CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

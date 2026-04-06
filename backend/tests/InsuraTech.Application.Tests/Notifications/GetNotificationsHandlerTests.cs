namespace InsuraTech.Application.Tests.Notifications;

using FluentAssertions;
using InsuraTech.Application.Notifications.Queries.GetNotifications;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using NSubstitute;

public sealed class GetNotificationsHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private GetNotificationsHandler Sut => new(_notificationRepository);

    private static Notification BuildPendingNotification() =>
        Notification.Create(Guid.NewGuid(), "Test User", "Subject", "Body", NotificationType.PolicyActivated);

    [Fact]
    public async Task Handle_WhenNotificationsExist_ShouldReturnPagedResult()
    {
        // GIVEN
        var notification = BuildPendingNotification();
        _notificationRepository.GetAllAsync(null, 1, 10, Arg.Any<CancellationToken>())
            .Returns((new[] { notification }.AsEnumerable(), 1));

        // WHEN
        var result = await Sut.Handle(new GetNotificationsQuery(), CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenNoNotificationsExist_ShouldReturnEmptyPagedResult()
    {
        // GIVEN
        _notificationRepository.GetAllAsync(null, 1, 10, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Notification>(), 0));

        // WHEN
        var result = await Sut.Handle(new GetNotificationsQuery(), CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallGetAllAsync()
    {
        // GIVEN
        _notificationRepository.GetAllAsync(null, 1, 10, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Notification>(), 0));

        // WHEN
        await Sut.Handle(new GetNotificationsQuery(), CancellationToken.None);

        // THEN
        await _notificationRepository.Received(1)
            .GetAllAsync(null, 1, 10, Arg.Any<CancellationToken>());
    }
}

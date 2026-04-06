namespace InsuraTech.Domain.Tests.Notifications;

using FluentAssertions;
using InsuraTech.Domain.Notifications;
using Xunit;

public sealed class NotificationTests
{
    private static Notification BuildPendingNotification() =>
        Notification.Create(
            Guid.NewGuid(), "Ana Lopez", "Policy Activated", "Your policy has been activated.",
            NotificationType.PolicyActivated);

    [Fact]
    public void Create_WithValidData_ShouldCreatePendingNotification()
    {
        // GIVEN / WHEN
        var notification = BuildPendingNotification();

        // THEN
        notification.Should().NotBeNull();
        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.Type.Should().Be(NotificationType.PolicyActivated);
        notification.Subject.Should().Be("Policy Activated");
        notification.RecipientName.Should().Be("Ana Lopez");
    }

    [Fact]
    public void MarkAsSent_WhenPending_ShouldSetStatusToSentAndRecordTime()
    {
        // GIVEN
        var notification = BuildPendingNotification();

        // WHEN
        notification.MarkAsSent();

        // THEN
        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAt.Should().NotBeNull();
        notification.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsFailed_WhenPending_ShouldSetStatusToFailedWithReason()
    {
        // GIVEN
        var notification = BuildPendingNotification();

        // WHEN
        notification.MarkAsFailed("SMTP connection refused");

        // THEN
        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.FailureReason.Should().Be("SMTP connection refused");
    }

    [Fact]
    public void Retry_WhenFailed_ShouldResetStatusToPending()
    {
        // GIVEN
        var notification = BuildPendingNotification();
        notification.MarkAsFailed("SMTP error");

        // WHEN
        notification.Retry();

        // THEN
        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.FailureReason.Should().BeNull();
        notification.SentAt.Should().BeNull();
    }

    [Fact]
    public void Retry_WhenNotFailed_ShouldThrowInvalidOperationException()
    {
        // GIVEN
        var notification = BuildPendingNotification();

        // WHEN
        var act = () => notification.Retry();

        // THEN
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only failed notifications can be retried*");
    }

    [Fact]
    public void Retry_WhenSent_ShouldThrowInvalidOperationException()
    {
        // GIVEN
        var notification = BuildPendingNotification();
        notification.MarkAsSent();

        // WHEN
        var act = () => notification.Retry();

        // THEN
        act.Should().Throw<InvalidOperationException>();
    }
}

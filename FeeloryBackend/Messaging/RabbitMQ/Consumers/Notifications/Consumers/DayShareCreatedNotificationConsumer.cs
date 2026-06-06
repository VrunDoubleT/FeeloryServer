using FeeloryBackend.Data;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Factories;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class DayShareCreatedNotificationConsumer
    : RabbitMqConsumerBase<DayShareCreatedMessage>
{
    public DayShareCreatedNotificationConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => NotificationQueues.DayShareCreated;

    protected override string RoutingKey => RoutingKeys.DayShareCreated;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        DayShareCreatedMessage message)
    {
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationCreatorService>();

        // Create notifications for all recipients except the author
        var notifications = message.RecipientIds
            .Distinct()
            .Where(userId => userId != message.AuthorId)
            .Select(userId =>
                NotificationFactory.Create(
                    userId,
                    message.AuthorId,
                    NotificationType.DayShareCreated,
                    message.DayShareId))
            .ToList();

        // Nothing to create
        if (notifications.Count == 0)
        {
            return;
        }

        await notificationService.CreateRangeAsync(notifications);
    }
}
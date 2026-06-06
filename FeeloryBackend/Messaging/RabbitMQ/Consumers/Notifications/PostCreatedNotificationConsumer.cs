using FeeloryBackend.Data;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostCreatedNotificationConsumer
    : RabbitMqConsumerBase<PostCreatedMessage>
{
    public PostCreatedNotificationConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => NotificationQueues.PostCreated;

    protected override string RoutingKey => NotificationRoutingKeys.PostCreated;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        PostCreatedMessage message)
    {
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationCreatorService>();

        // Create notifications for all recipients except the author
        var notifications = message.RecipientIds
            .Distinct()
            .Where(userId => userId != message.AuthorId)
            .Select(userId =>
                NotificationFactory.Create(
                    userId: userId,
                    actorId: message.AuthorId,
                    type: NotificationType.PostCreated,
                    targetId: message.PostId))
            .ToList();

        // Nothing to create
        if (notifications.Count == 0)
        {
            return;
        }

        await notificationService.CreateRangeAsync(notifications);
    }
}
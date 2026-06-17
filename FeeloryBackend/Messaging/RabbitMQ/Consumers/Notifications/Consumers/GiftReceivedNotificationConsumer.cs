
using FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Factories;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Consumers;

public class GiftReceivedNotificationConsumer
    : RabbitMqConsumerBase<GiftReceivedMessage>
{
    public GiftReceivedNotificationConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => NotificationQueues.GiftReceived;

    protected override string RoutingKey => NotificationRoutingKeys.GiftReceived;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        GiftReceivedMessage message)
    {
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationCreatorService>();

        var notification = NotificationFactory.Create(
            message.UserId,
            null,
            NotificationType.GiftReceived,
            message.MissionId);

        await notificationService.CreateAsync(notification);
    }
}
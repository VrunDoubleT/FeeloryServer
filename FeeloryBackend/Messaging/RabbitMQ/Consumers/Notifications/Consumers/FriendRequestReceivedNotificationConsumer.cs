using FeeloryBackend.Data;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Factories;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Consumers;

public class FriendRequestReceivedNotificationConsumer
    : RabbitMqConsumerBase<FriendRequestReceivedMessage>
{
    public FriendRequestReceivedNotificationConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => NotificationQueues.FriendRequestReceived;

    protected override string RoutingKey => NotificationRoutingKeys.FriendRequestReceived;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        FriendRequestReceivedMessage message)
    {
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationCreatorService>();

        var notification = NotificationFactory.Create(
            message.ReceiverId,
            message.SenderId,
            NotificationType.FriendRequestReceived,
            message.FriendRequestId);

        await notificationService.CreateAsync(notification);
    }
}
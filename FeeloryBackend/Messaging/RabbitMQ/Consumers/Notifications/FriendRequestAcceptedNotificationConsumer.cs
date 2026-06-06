using FeeloryBackend.Data;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class FriendRequestAcceptedNotificationConsumer
    : RabbitMqConsumerBase<FriendRequestAcceptedMessage>
{
    public FriendRequestAcceptedNotificationConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => NotificationQueues.FriendRequestAccepted;

    protected override string RoutingKey => NotificationRoutingKeys.FriendRequestAccepted;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        FriendRequestAcceptedMessage message)
    {
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationCreatorService>();

        var notification = NotificationFactory.Create(
            message.SenderId,
            message.AccepterId,
            NotificationType.FriendRequestAccepted,
            message.FriendRequestId);
        
        await notificationService.CreateAsync(notification);
    }
}
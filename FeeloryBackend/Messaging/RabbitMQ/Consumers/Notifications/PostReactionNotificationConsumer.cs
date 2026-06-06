using FeeloryBackend.Data;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostReactionNotificationConsumer
    : RabbitMqConsumerBase<PostReactionAddedMessage>
{
    public PostReactionNotificationConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => NotificationQueues.PostReactionAdded;

    protected override string RoutingKey => NotificationRoutingKeys.PostReactionAdded;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        PostReactionAddedMessage message)
    {
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationCreatorService>();

        // Do not notify when reacting to own post
        if (message.ReactorId == message.OwnerId)
        {   
            return;
        }

        var notification = NotificationFactory.Create(
            userId: message.OwnerId,
            actorId: message.ReactorId,
            type: NotificationType.PostReactionAdded,
            targetId: message.PostId,
            metadata: new
            {
                message.ReactionCode
            });
        
        await notificationService.CreateAsync(notification);
    }
}
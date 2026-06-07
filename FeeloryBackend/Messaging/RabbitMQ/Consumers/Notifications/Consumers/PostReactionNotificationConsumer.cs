using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Reactions;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Consumers;

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

    protected override string RoutingKey => RoutingKeys.PostReactionAdded;

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

        await notificationService.CreateOrUpdateReactionAsync(
            ownerId: message.OwnerId,
            reactorId: message.ReactorId,
            postId: message.PostId,
            emoteId: message.EmoteId);
    }
}
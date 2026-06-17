using FeeloryBackend.Messaging.RabbitMQ.Messages.Reactions;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Missions;

public class ReactionReceivedMissionConsumer : RabbitMqConsumerBase<PostReactionAddedMessage>
{
    public ReactionReceivedMissionConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName =>
        MissionQueues.ReactionReceived;

    protected override string RoutingKey => RoutingKeys.PostReactionAdded;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        PostReactionAddedMessage message)
    {
        var missionService = scope.ServiceProvider.GetRequiredService<IMissionProgressService>();

        await missionService.ProcessReactionReceivedAsync(
            message.OwnerId,
            message.ReactorId,
            message.PostId);
    }
}
using FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Missions;

public class DayShareMissionConsumer : RabbitMqConsumerBase<DayShareCreatedMessage>
{
    public DayShareMissionConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => MissionQueues.DayShareCreated;

    protected override string RoutingKey => RoutingKeys.DayShareCreated;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        DayShareCreatedMessage message)
    {
        var missionService = scope.ServiceProvider.GetRequiredService<IMissionProgressService>();

        await missionService.ProcessDayShareCreatedAsync(
            message.AuthorId,
            message.DayShareId);
    }
}
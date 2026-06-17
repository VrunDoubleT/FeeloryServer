using FeeloryBackend.Messaging.RabbitMQ.Messages.Auth;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Missions;

public class LoginMissionConsumer : RabbitMqConsumerBase<LoginHeartbeatMessage>
{
    public LoginMissionConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => MissionQueues.Login;

    protected override string RoutingKey => RoutingKeys.LoginHeartbeat;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        LoginHeartbeatMessage message)
    {
        var missionService = scope.ServiceProvider.GetRequiredService<IMissionProgressService>();
        await missionService.ProcessLoginAsync(message.UserId);
    }
}
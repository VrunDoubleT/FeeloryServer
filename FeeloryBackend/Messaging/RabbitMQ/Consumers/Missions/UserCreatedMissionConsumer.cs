using FeeloryBackend.Messaging.RabbitMQ.Messages.Users;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Missions;

public class UserCreatedMissionConsumer 
    : RabbitMqConsumerBase<UserCreatedMessage>
{
    public UserCreatedMissionConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => MissionQueues.UserCreated;
    
    protected override string RoutingKey => RoutingKeys.UserCreated;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        UserCreatedMessage message)
    {
        var missionInitializationService = scope.ServiceProvider
                .GetRequiredService<IMissionInitializationService>();
        
        await missionInitializationService
            .InitializeUserMissionsAsync(message.UserId);
    }
}
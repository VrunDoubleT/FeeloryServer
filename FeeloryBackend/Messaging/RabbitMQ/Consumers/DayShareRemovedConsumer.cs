using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Implementations;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class DayShareRemovedConsumer : RabbitMqConsumerBase<DayShareFeedMessage>
{
    public DayShareRemovedConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory) { }

    protected override string QueueName  => QueueNames.DayShareRemoved;
    protected override string RoutingKey => RoutingKeys.DayShareRemoved;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        DayShareFeedMessage message)
    {
        var service = scope.ServiceProvider
            .GetRequiredService<IDayShareFeedService>();
        
        await service.HandleRemovedAsync(message);
    }
}
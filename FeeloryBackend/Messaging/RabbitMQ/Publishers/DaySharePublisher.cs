using FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class DaySharePublisher
{
    private readonly IEventBus _eventBus;

    public DaySharePublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <summary>
    /// Publish a day share created message
    /// </summary>
    public async Task PublishDayShareCreatedAsync(DayShareCreatedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.DayShareCreated, message);
    }

    /// <summary>
    /// Publish a day share updated message
    /// </summary>
    public async Task PublishDayShareUpdatedAsync(DayShareUpdatedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.DayShareUpdated, message);
    }

    /// <summary>
    /// Publish a day share deleted message
    /// </summary>
    public async Task PublishDayShareDeletedAsync(DayShareDeletedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.DayShareDeleted, message);
    }
}
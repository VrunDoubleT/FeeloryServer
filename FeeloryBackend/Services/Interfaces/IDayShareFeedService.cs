using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;

namespace FeeloryBackend.Services.Interfaces;

public interface IDayShareFeedService
{
    Task HandleAddFeedsAsync(
        DayShareFeedMessage message);

    Task HandleRemovedAsync(
        DayShareFeedMessage message);

    Task HandleDeletedAsync(
        DayShareFeedMessage message);
}
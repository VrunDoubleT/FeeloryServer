using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;

public class EmailPublisher
{
    private readonly IEventBus _eventBus;

    public EmailPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task SendEmailAsync(EmailMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.EmailSend, message);
    }
}
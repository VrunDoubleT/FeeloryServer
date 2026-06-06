using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Email;

public class EmailConsumerService : RabbitMqConsumerBase<EmailMessage>
{
    public EmailConsumerService(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => QueueNames.Email;

    protected override string RoutingKey => RoutingKeys.EmailSend;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        EmailMessage message)
    {
        Console.WriteLine($"📧 Send email to: {message.To}");

        var emailService = scope.ServiceProvider
            .GetRequiredService<IEmailService>();

        await emailService.SendEmailAsync(
            message.To,
            message.Subject,
            message.Body);
    }
}
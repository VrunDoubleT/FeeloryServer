using System.Text;
using System.Text.Json;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class EmailConsumerService : BackgroundService
{
    private readonly IRabbitMQConnectionFactory _factory;
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateConnection();
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConstants.MainExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        var queueName = "email_queue";

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: RabbitMQConstants.MainExchange,
            routingKey: RoutingKeys.EmailSend,
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<EmailMessage>(json);

                Console.WriteLine($"[RabbitMQ] RoutingKey={args.RoutingKey}");
                Console.WriteLine($"📧 Send email to: {message?.To}");
                
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                
                await emailService.SendEmailAsync(message?.To, message?.Subject, message?.Body);

                await channel.BasicAckAsync(args.DeliveryTag, false, stoppingToken);
            }
            catch
            {
                await channel.BasicNackAsync(args.DeliveryTag, false, true, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            cancellationToken: stoppingToken
        );

        // Hold service live
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
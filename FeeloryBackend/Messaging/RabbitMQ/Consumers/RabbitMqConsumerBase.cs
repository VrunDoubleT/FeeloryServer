using System.Text;
using System.Text.Json;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public abstract class RabbitMqConsumerBase<TMessage> : BackgroundService
    where TMessage : class
{
    private readonly IRabbitMQConnectionFactory _factory;
    private readonly IServiceScopeFactory _scopeFactory;

    protected RabbitMqConsumerBase(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
    }

    protected abstract string QueueName { get; }

    protected abstract string RoutingKey { get; }

    protected abstract Task ProcessAsync(
        IServiceScope scope,
        TMessage message);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection =
                    await _factory.CreateConnection();

                await using var channel =
                    await connection.CreateChannelAsync(
                        cancellationToken: stoppingToken);

                await channel.ExchangeDeclareAsync(
                    exchange: RabbitMQConstants.MainExchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: QueueName,
                    exchange: RabbitMQConstants.MainExchange,
                    routingKey: RoutingKey,
                    cancellationToken: stoppingToken);

                var consumer =
                    new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (_, args) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(
                            args.Body.ToArray());

                        var message =
                            JsonSerializer.Deserialize<TMessage>(json);

                        if (message is null)
                        {
                            await channel.BasicAckAsync(
                                args.DeliveryTag,
                                false,
                                stoppingToken);

                            return;
                        }

                        using var scope = _scopeFactory.CreateScope();

                        await ProcessAsync(scope, message);

                        await channel.BasicAckAsync(
                            args.DeliveryTag,
                            false,
                            stoppingToken);
                    }
                    catch
                    {
                        await channel.BasicNackAsync(
                            args.DeliveryTag,
                            false,
                            true,
                            stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{GetType().Name}] Lost connection: {ex.Message}");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
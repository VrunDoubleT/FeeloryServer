namespace FeeloryBackend.Extensions;

using FeeloryBackend.Messaging.RabbitMQ;
using FeeloryBackend.Messaging.RabbitMQ.Consumers;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class RabbitMQExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind config
        services.Configure<RabbitMQSettings>(
            configuration.GetSection("RabbitMQ"));

        // Core infrastructure
        services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        // Publishers
        services.AddSingleton<EmailPublisher>();
        services.AddSingleton<PostPublisher>();

        services.AddScoped<DaySharePublisher>();
        // Consumers (Background Services)
        services.AddHostedService<EmailConsumerService>();
        services.AddHostedService<PostCreatedConsumerService>();
        services.AddHostedService<PostAddedConsumerService>();
        services.AddHostedService<PostRemovedConsumerService>();
        services.AddHostedService<PostDeletedConsumerService>();

        services.AddHostedService<DayShareCreatedConsumer>();
        services.AddHostedService<DayShareUpdatedConsumer>();
        services.AddHostedService<DayShareRemovedConsumer>();
        services.AddHostedService<DayShareDeletedConsumer>();

        services.AddScoped<ReactionPublisher>();
        // Consumers (Background Services)
        services.AddHostedService<EmailConsumerService>();
    
        services.AddHostedService<ReactionNotificationConsumer>();
        services.AddHostedService<ReactionTaskConsumer>();

        return services;
    }
}
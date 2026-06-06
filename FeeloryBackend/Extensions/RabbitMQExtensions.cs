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
        services.AddScoped<ReactionPublisher>();
        services.AddScoped<DaySharePublisher>();
        services.AddScoped<NotificationPublisher>();
        
        // Consumers (Background Services)
        // Email
        services.AddHostedService<EmailConsumerService>();
        // Post
        services.AddHostedService<PostAddedConsumerService>();
        services.AddHostedService<PostRemovedConsumerService>();
        services.AddHostedService<PostDeletedConsumerService>();
        // DayShare
        services.AddHostedService<DayShareAddedConsumer>();
        services.AddHostedService<DayShareRemovedConsumer>();
        services.AddHostedService<DayShareDeletedConsumer>();
        //Notification
        services.AddHostedService<PostCreatedNotificationConsumer>();
        services.AddHostedService<PostReactionNotificationConsumer>();
        services.AddHostedService<DayShareCreatedNotificationConsumer>();
        services.AddHostedService<FriendRequestReceivedNotificationConsumer>();
        services.AddHostedService<FriendRequestAcceptedNotificationConsumer>();
        services.AddHostedService<MissionCompletedNotificationConsumer>();
        services.AddHostedService<GiftReceivedNotificationConsumer>();
        
        // services.AddHostedService<ReactionNotificationConsumer>();

        return services;
    }
}
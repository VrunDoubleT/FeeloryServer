using FeeloryBackend.Messaging.RabbitMQ.Consumers.DayShareFeeds;
using FeeloryBackend.Messaging.RabbitMQ.Consumers.Email;
using FeeloryBackend.Messaging.RabbitMQ.Consumers.Histories;
using FeeloryBackend.Messaging.RabbitMQ.Consumers.Missions;
using FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Consumers;
using FeeloryBackend.Messaging.RabbitMQ.Consumers.PostFeeds;

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
        services.AddSingleton<PostReactionPublisher>();
        services.AddSingleton<DaySharePublisher>();
        services.AddSingleton<NotificationPublisher>();
        services.AddSingleton<HeartbeatPublisher>();
        
        // Consumers (Background Services)
        // Email
        services.AddHostedService<EmailConsumerService>();
        // Post
        services.AddHostedService<PostCreatedFeedConsumer>();
        services.AddHostedService<PostDeletedFeedConsumer>();
        services.AddHostedService<PostUpdatedFeedAddedConsumer>();
        services.AddHostedService<PostUpdatedFeedRemovedConsumer>();
        // DayShare
        services.AddHostedService<DayShareCreatedFeedConsumer>();
        services.AddHostedService<DayShareDeletedFeedConsumer>();
        services.AddHostedService<DayShareUpdatedFeedAddedConsumer>();
        services.AddHostedService<DayShareUpdatedFeedRemovedConsumer>();
        //Notification
        services.AddHostedService<PostCreatedNotificationConsumer>();
        services.AddHostedService<DayShareCreatedNotificationConsumer>();
        services.AddHostedService<PostReactionNotificationConsumer>();
        services.AddHostedService<FriendRequestReceivedNotificationConsumer>();
        services.AddHostedService<FriendRequestAcceptedNotificationConsumer>();
        services.AddHostedService<MissionCompletedNotificationConsumer>();
        services.AddHostedService<GiftReceivedNotificationConsumer>();
        // Missions
        services.AddHostedService<DayShareMissionConsumer>();
        services.AddHostedService<ReactionSentMissionConsumer>();
        services.AddHostedService<ReactionReceivedMissionConsumer>();
        services.AddHostedService<LoginMissionConsumer>();
        services.AddHostedService<LoginHistoryConsumer>();
        return services;
    }
}
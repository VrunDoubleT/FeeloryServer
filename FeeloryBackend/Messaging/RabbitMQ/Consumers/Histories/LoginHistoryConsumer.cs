using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Auth;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Histories;

public class LoginHistoryConsumer
    : RabbitMqConsumerBase<LoginHeartbeatMessage>
{
    public LoginHistoryConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => MissionQueues.LoginHistory;

    protected override string RoutingKey => RoutingKeys.LoginHeartbeat;

    protected override async Task ProcessAsync(
        IServiceScope scope,
        LoginHeartbeatMessage message)
    {
        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var exists = await db.UserLoginHistories
            .AnyAsync(x =>
                x.UserId == message.UserId &&
                x.LoginDate == message.LoginDate);

        if (exists)
        {
            return;
        }

        db.UserLoginHistories.Add(
            new UserLoginHistory
            {
                Id = Guid.NewGuid(),
                UserId = message.UserId,
                LoginDate = message.LoginDate,
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();
    }
}
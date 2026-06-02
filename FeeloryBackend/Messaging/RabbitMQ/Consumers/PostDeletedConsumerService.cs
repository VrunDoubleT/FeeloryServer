using System.Text;
using System.Text.Json;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ExchangeType = RabbitMQ.Client.ExchangeType;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostDeletedConsumerService : PostConsumerService
{
    private readonly IPostFeedService  _postFeedService;

    public PostDeletedConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory,
        IPostFeedService postFeedService)
        : base(factory, scopeFactory)
    {
        _postFeedService = postFeedService;
    }

    protected override string QueueName => QueueNames.PostDeleted;
    protected override string RoutingKey => RoutingKeys.PostDeleted;
    protected override string Action => PostMessage.ActionDeleted;

    protected override Task ProcessAsync(AppDbContext db, PostMessage message)
        => _postFeedService.HandleDeletePostAsync(db, message);
}
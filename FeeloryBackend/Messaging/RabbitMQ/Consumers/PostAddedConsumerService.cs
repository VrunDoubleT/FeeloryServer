using System.Text;
using System.Text.Json;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ExchangeType = RabbitMQ.Client.ExchangeType;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostAddedConsumerService : PostConsumerService
{
    private readonly IPostFeedService  _postFeedService;

    public PostAddedConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory, IPostFeedService postFeedService)
        : base(factory, scopeFactory)
    {
        _postFeedService = postFeedService;
    }

    protected override string QueueName => QueueNames.PostPermissionAdded;
    protected override string RoutingKey => RoutingKeys.PostPermissionAdded;
    protected override string Action => PostMessage.ActionAdded;

    protected override Task ProcessAsync(AppDbContext db, PostMessage message)
        => _postFeedService.HandleAddFeedsAsync(db, message);
}
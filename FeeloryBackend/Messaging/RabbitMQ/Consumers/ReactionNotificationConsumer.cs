// using System.Text;
// using System.Text.Json;
// using FeeloryBackend.Data;
// using FeeloryBackend.Messaging.RabbitMQ.Constants;
// using FeeloryBackend.Messaging.RabbitMQ.Messages;
// using FeeloryBackend.Messaging.RabbitMQ.Queues;
// using FeeloryBackend.Messaging.RabbitMQ.Routing;
// using FeeloryBackend.Models.Entities;
// using Microsoft.EntityFrameworkCore;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;
// using Task = System.Threading.Tasks.Task;
//
// namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;
//
// public class ReactionNotificationConsumer : BackgroundService
// {
//     private readonly IRabbitMQConnectionFactory _factory;
//     private readonly IServiceScopeFactory _scopeFactory;
//
//     public ReactionNotificationConsumer(
//         IRabbitMQConnectionFactory factory,
//         IServiceScopeFactory scopeFactory)
//     {
//         _factory = factory;
//         _scopeFactory = scopeFactory;
//     }
//
//     protected override async Task ExecuteAsync(
//         CancellationToken stoppingToken)
//     {
//         var connection = await _factory.CreateConnection();
//
//         var channel = await connection.CreateChannelAsync(
//             cancellationToken: stoppingToken);
//
//         await channel.ExchangeDeclareAsync(
//             exchange: RabbitMQConstants.MainExchange,
//             type: ExchangeType.Topic,
//             durable: true,
//             autoDelete: false,
//             cancellationToken: stoppingToken);
//
//         await channel.QueueDeclareAsync(
//             queue: QueueNames.Reaction,
//             durable: true,
//             exclusive: false,
//             autoDelete: false,
//             cancellationToken: stoppingToken);
//
//         await channel.QueueBindAsync(
//             queue: QueueNames.Reaction,
//             exchange: RabbitMQConstants.MainExchange,
//             routingKey: RoutingKeys.Reaction,
//             cancellationToken: stoppingToken);
//
//         var consumer = new AsyncEventingBasicConsumer(channel);
//
//         consumer.ReceivedAsync += async (_, args) =>
//         {
//             try
//             {
//                 var json = Encoding.UTF8.GetString(args.Body.ToArray());
//
//                 var message = JsonSerializer.Deserialize<ReactionMessage>(json);
//
//                 if (message is null)
//                 {
//                     await channel.BasicAckAsync(
//                         args.DeliveryTag,
//                         false,
//                         stoppingToken);
//
//                     return;
//                 }
//
//                 using var scope = _scopeFactory.CreateScope();
//
//                 var db = scope.ServiceProvider
//                     .GetRequiredService<AppDbContext>();
//
//                 var notificationType = await db.NotificationTypes
//                     .FirstOrDefaultAsync(
//                         x => x.Code == message.Action,
//                         stoppingToken);
//
//                 if (notificationType is null)
//                 {
//                     await channel.BasicAckAsync(
//                         args.DeliveryTag,
//                         false,
//                         stoppingToken);
//
//                     return;
//                 }
//
//                 string notificationMessage = message.Action switch
//                 {
//                     ReactionMessage.ActionPostReacted =>
//                         $"{message.ReactorName} reacted to your post.",
//                     
//
//                     _ => "New reaction"
//                 };
//
//                 db.Notifications.Add(new Notification
//                 {
//                     Id = Guid.NewGuid(),
//                     UserId = message.TargetOwnerId,
//                     TypeId = notificationType.Id,
//                     Title = "New Reaction",
//                     Message = notificationMessage,
//                     IsRead = false,
//                     CreatedAt = DateTime.UtcNow
//                 });
//
//                 await db.SaveChangesAsync(stoppingToken);
//
//                 await channel.BasicAckAsync(
//                     args.DeliveryTag,
//                     false,
//                     stoppingToken);
//             }
//             catch
//             {
//                 await channel.BasicNackAsync(
//                     args.DeliveryTag,
//                     false,
//                     true,
//                     stoppingToken);
//             }
//         };
//
//         await channel.BasicConsumeAsync(
//             queue: QueueNames.Reaction,
//             autoAck: false,
//             consumer: consumer,
//             cancellationToken: stoppingToken);
//
//         await Task.Delay(
//             Timeout.Infinite,
//             stoppingToken);
//     }
// }
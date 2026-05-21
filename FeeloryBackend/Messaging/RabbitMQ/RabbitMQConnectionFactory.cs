using RabbitMQ.Client;

namespace FeeloryBackend.Messaging.RabbitMQ;

using Microsoft.Extensions.Options;

public interface IRabbitMQConnectionFactory
{
    Task<IConnection> CreateConnection();
}

public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory
{
    private readonly RabbitMQSettings _settings;

    public RabbitMQConnectionFactory(IOptions<RabbitMQSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<IConnection> CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        return await factory.CreateConnectionAsync();
    }
}
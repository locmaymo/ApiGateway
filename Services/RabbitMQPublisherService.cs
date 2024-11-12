using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using ApiGateway.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Mail;

namespace ApiGateway.Services
{
    public class RabbitMQPublisherService
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly ILogger<RabbitMQPublisherService> _logger;

        public RabbitMQPublisherService(IOptions<RabbitMQSettings> options, ILogger<RabbitMQPublisherService> logger)
        {
            _rabbitMQSettings = options.Value;
            _logger = logger;

            _factory = new ConnectionFactory()
            {
                HostName = _rabbitMQSettings.HostName,
                UserName = _rabbitMQSettings.UserName,
                Password = _rabbitMQSettings.Password,
                VirtualHost = _rabbitMQSettings.VirtualHost,
                Port = _rabbitMQSettings.Port
            };
        }

        public async Task PublishEmailMessageAsync(EmailModel emailMessage)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var connection = _factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        var queueName = _rabbitMQSettings.QueueName;

                        // Declare the queue if it doesn't exist
                        channel.QueueDeclare(queue: queueName,
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        string messageJson = JsonConvert.SerializeObject(emailMessage);
                        var body = Encoding.UTF8.GetBytes(messageJson);

                        var properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = 2; // Persistent

                        channel.BasicPublish(exchange: "",
                                             routingKey: queueName,
                                             basicProperties: properties,
                                             body: body);

                        _logger.LogInformation("Email message published to RabbitMQ.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing email message to RabbitMQ.");
                    throw;
                }
            });
        }
    }
}

using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using System.Threading.Tasks;
using orderConsumerAPI.services;

namespace orderConsumerAPI.services
{
    public class ConsumeRabbitMQHostedService : IConsumeRabbitMQHostedService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        ConnectionFactory _factory;

        public ConsumeRabbitMQHostedService()
        {
            //this._logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedService>();
            InitRabbitMQ();
        }

        public  void InitRabbitMQ()
        {
            _factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
            _factory.UserName = "admin";
            _factory.Password = "101010";
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            // _channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            // _channel.QueueDeclare("demo.queue.log", false, false, false, null);
            // _channel.QueueBind("demo.queue.log", "demo.exchange", "demo.queue.*", null);
            // _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        public  void ExecuteAsync()
        { 

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(body);

                // handle the received message  
                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("pedidos", false, consumer);
            //return Task.CompletedTask;
        }

        private void HandleMessage(string content)
        {
            // we just print this message   
            _logger.LogInformation($"consumer received {content}");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        void IConsumeRabbitMQHostedService.InitRabbitMQ()
        {
            throw new System.NotImplementedException();
        }
    }
}
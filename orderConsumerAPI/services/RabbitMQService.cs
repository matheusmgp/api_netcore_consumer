using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using orderProducerAPI.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace orderConsumerAPI.services
{
    public class RabbitMQService : IHostedService, IRabbitMQService
    {
        private string tag2;
        private string consumerTag;
        private EventingBasicConsumer _consumer;
        private static string TAG_CONSUMER = "";

        private static IModel _myChannel;
        private IConnection _connection;      
        ConnectionFactory _factory;
        private static bool IsRunning;

        private string BASE_URL = "localhost";
        private string USERNAME = "admin";
        private string PASSWORD = "101010";

        //private string BASE_URL_DO_WORK_LOCAL = "https://localhost:44387";
       // private string BASE_URL_DO_WORK_HOMOLOGACAO = "http://srvvapp016:8098";
        private string BASE_URL_DO_WORK = "";

        private readonly ILogger<RabbitMQService> _logger;
        private readonly string MY_QUEUE = "pedidos";       

        public RabbitMQService(ILogger<RabbitMQService> logger)
        {
            _logger = logger;
            InitRabbitMQ();


        }
        public void InitRabbitMQ()
        {
            _factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
            _factory.UserName = "admin";
            _factory.Password = "101010";
            _connection = _factory.CreateConnection();
            _myChannel = _connection.CreateModel();
          
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StartAsync Logging...");
            DoWork();
            return Task.CompletedTask;
        }

        public async Task DoWork()
        {
            IsRunning = true;

            var factory = new ConnectionFactory()
            {
                HostName = BASE_URL,
                UserName = USERNAME,
                Password = PASSWORD,
                VirtualHost = "/",
                Port = AmqpTcpEndpoint.UseDefaultPort
            };
            using (var connection = factory.CreateConnection())
            using (_myChannel = connection.CreateModel())
            {
                _myChannel.QueueDeclare(queue: MY_QUEUE,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                _consumer = new EventingBasicConsumer(_myChannel);
                _consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    try
                    {
                        _logger.LogInformation($"Recebido => Order. => {message}");

                        // Json to Object
                        Order myObject = JsonSerializer.Deserialize<Order>(message);

                        _logger.LogInformation($"Iniciando DoWork => Order. => {message}");

                        var resultDoWork = await CallApiToDoWork(myObject);

                        _logger.LogInformation($"Retorno: DoWork => {resultDoWork} Order. => {message}");

                        // Informa que a mensagem foi recebida com sucesso depois do processamento
                        _myChannel.BasicAck(ea.DeliveryTag, false);

                        _logger.LogInformation($"DoWork Finish => Order. => {message}");
                    }
                    catch (OperationCanceledException o)
                    {
                        // Reenviar para a Fila se for essa exception:
                        _myChannel.BasicReject(ea.DeliveryTag, true);
                        _logger.LogInformation($"Erro ao processar a Mensagem Order. => {message}");
                        _logger.LogInformation($"Erro: {o.Message} Order. => {message}");
                    }
                    catch (Exception e)
                    {
                        // Informa que a mensagem foi rejeitada se houver algum erro
                        _myChannel.BasicReject(ea.DeliveryTag, false);
                        _logger.LogInformation($"Erro ao processar a Mensagem Order. => {message}");
                        _logger.LogInformation($"Erro: {e.Message} Order. => {message}");
                    }
                };

                TAG_CONSUMER = _myChannel.BasicConsume(queue: MY_QUEUE,
                                     autoAck: false,
                                     consumer: _consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        public void StopWork()
        {
            var factory = new ConnectionFactory()
            {
                HostName = BASE_URL,
                UserName = USERNAME,
                Password = PASSWORD,
                VirtualHost = "/",
                Port = AmqpTcpEndpoint.UseDefaultPort
            };
            using (var connection = factory.CreateConnection())
            using (_myChannel)
            {
                _myChannel.BasicCancel(TAG_CONSUMER);
            }

            IsRunning = false;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (IsRunning)
            {
                StopWork();
                Console.WriteLine($"Service finished {nameof(RabbitMQService)}");
            }
            else
            {
                Console.WriteLine($"Service finished already! {nameof(RabbitMQService)}");
            }


            return Task.CompletedTask;
        }

        private async Task<String> CallApiToDoWork(Order model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    StringContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                    // Link do Azix de PRD:
                    using (var response = await client.PostAsync($"{BASE_URL_DO_WORK}/api/RunSolicitacaoPreClienteDMAuto/RunSolicitacaoPreClienteDMAuto", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            return apiResponse;
                        }
                        else
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            return $"Erro no DoWork: {apiResponse}";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

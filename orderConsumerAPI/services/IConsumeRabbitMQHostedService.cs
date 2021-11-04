using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace orderConsumerAPI.services
{
    public interface IConsumeRabbitMQHostedService
    {
        public  void InitRabbitMQ();
        public  void ExecuteAsync();
    }
}

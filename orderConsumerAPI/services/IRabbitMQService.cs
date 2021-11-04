using System.Threading;
using System.Threading.Tasks;

namespace orderConsumerAPI.services
{
    public interface IRabbitMQService
    {
        Task DoWork();
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        void StopWork();
    }
}

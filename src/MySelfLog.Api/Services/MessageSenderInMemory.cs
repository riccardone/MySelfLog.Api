using System.Collections.Concurrent;
using System.Threading.Tasks;
using MySelfLog.Contracts;
using MySelfLog.Contracts.Api;

namespace MySelfLog.Api.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageSenderInMemory : IMessageSender
    {
        private readonly ConcurrentQueue<dynamic> _queue = new ConcurrentQueue<dynamic>();
        public ConcurrentQueue<dynamic> Queue => _queue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        public Task SendAsync(CloudEventRequest[] requests)
        {
            foreach (var cloudEventRequest in requests)
            {
                _queue.Enqueue(cloudEventRequest);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task SendAsync(CloudEventRequest request)
        {
            _queue.Enqueue(request);
            return Task.CompletedTask;
        }
    }
}

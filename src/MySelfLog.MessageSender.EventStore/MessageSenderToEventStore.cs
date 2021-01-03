using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using MySelfLog.Contracts;
using MySelfLog.Contracts.Api;
using Newtonsoft.Json;

namespace MySelfLog.MessageSender.EventStore
{
    /// <summary>
    /// Send messages to Azure Service bus queue
    /// </summary>
    public class MessageSenderToEventStore : IMessageSender
    {
        private readonly IConnectionBuilder _connectionBuilder;

        /// <summary>
        /// Send messages to Azure Service bus queue
        /// </summary>
        public MessageSenderToEventStore(IBusSettings busSettings)
        {
            _connectionBuilder = new ConnectionBuilder(new Uri(busSettings.Link), ConnectionSettings.Default, busSettings.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        public async Task SendAsync(CloudEventRequest[] requests)
        {
            foreach (var cloudEventRequest in requests)
                await SendAsync(cloudEventRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task SendAsync(CloudEventRequest request)
        {
            using var conn = _connectionBuilder.Build();
            conn.ConnectAsync().Wait();
            var streamName = request.Source.IsAbsoluteUri ? request.Source.Host : request.Source.ToString();
            await conn.AppendToStreamAsync($"datainput-{streamName}-{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month}",
                ExpectedVersion.Any, CreateEventData(request, request.Type));
        }

        private EventData CreateEventData(object obj, string eventType)
        {
            var data = SerializeObject(obj);
            var eventData = new EventData(Guid.NewGuid(), eventType, true, data, null);
            return eventData;
        }

        private static byte[] SerializeObject(object obj)
        {
            var jsonObj = JsonConvert.SerializeObject(obj);
            var data = Encoding.UTF8.GetBytes(jsonObj);
            return data;
        }
    }
}

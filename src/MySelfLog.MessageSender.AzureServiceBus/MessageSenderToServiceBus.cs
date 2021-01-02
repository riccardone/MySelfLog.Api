using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using MySelfLog.Contracts;
using MySelfLog.Contracts.Api;
using Newtonsoft.Json;

namespace MySelfLog.MessageSender.AzureServiceBus
{
    /// <summary>
    /// Send messages to Azure Service bus queue
    /// </summary>
    public class MessageSenderToServiceBus : IMessageSender
    {
        private readonly IBusSettings _busSettings;

        /// <summary>
        /// Send messages to Azure Service bus queue
        /// </summary>
        /// <param name="busSettings"></param>
        public MessageSenderToServiceBus(IBusSettings busSettings)
        {
            _busSettings = busSettings;
        }

        /// <summary>
        /// Send a batch of messages to Azure Service Bus
        /// </summary>
        /// <param name="requests">the messages to be sent</param>
        public async Task SendAsync(CloudEventRequest[] requests)
        {
            foreach (var request in requests)
            {
                await SendAsync(request);
            }
        }

        /// <summary>
        /// Send one message to Azure Service Bus
        /// </summary>
        /// <param name="request">the message to be sent</param>
        public async Task SendAsync(CloudEventRequest request)
        {
            var connStringBuilder = new ServiceBusConnectionStringBuilder(_busSettings.Link);
            var busClient = new Microsoft.Azure.ServiceBus.Core.MessageSender(connStringBuilder);
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));
            await busClient.SendAsync(message);
        }
    }
}

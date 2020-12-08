using System;
using System.Threading.Tasks;
using MySelfLog.Contracts;
using MySelfLog.Contracts.Api;

namespace MySelfLog.MessageSender.EventStore
{
    public class MessageSenderToEventStore : IMessageSender
    {
        public Task SendAsync(CloudEventRequest[] requests)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(CloudEventRequest request)
        {
            throw new NotImplementedException();
        }
    }
}

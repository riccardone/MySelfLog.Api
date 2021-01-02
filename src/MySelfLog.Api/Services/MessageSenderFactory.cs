using System;
using Finbuckle.MultiTenant;
using Microsoft.Extensions.Logging;
using MySelfLog.Admin.Model;
using MySelfLog.Contracts.Api;
using MySelfLog.MessageSender.AzureServiceBus;
using MySelfLog.MessageSender.EventStore;

namespace MySelfLog.Api.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageSenderFactory : IMessageSenderFactory
    {
        private readonly ILogger<MessageSenderFactory> _logger;
        private readonly IMultiTenantStore<MySelfLogTenantInfo> _store;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="store"></param>
        public MessageSenderFactory(ILogger<MessageSenderFactory> logger, IMultiTenantStore<MySelfLogTenantInfo> store)
        {
            _logger = logger;
            _store = store;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IMessageSender Build(string source)
        {
            if (_store == null)
                throw new Exception("Store is null");

            var tenant = _store.TryGetByIdentifierAsync(source).Result;
            if (tenant == null)
                throw new Exception($"I can't find a configured tenant for this source");
            if (string.IsNullOrEmpty(tenant.MessageBusLink))
            {
                _logger.LogWarning($"Configuring '{nameof(MessageSenderInMemory)}' only for testing!");
                return new MessageSenderInMemory();
            }
            if (tenant.MessageBusLink.StartsWith("Endpoint=sb"))
            {
                _logger.LogWarning($"Configuring '{nameof(MessageSenderToServiceBus)}'");
                return new MessageSenderToServiceBus(new BusSettings(tenant.MessageBusLink, tenant.Identifier));
            }
            if (tenant.MessageBusLink.StartsWith($"tcp"))
            {
                _logger.LogInformation($"Configuring '{nameof(MessageSenderToEventStore)}'");
                return new MessageSenderToEventStore(new BusSettings(tenant.MessageBusLink, tenant.Identifier));
            }
            throw new Exception("MessageBusLink not recognized");
        }
    }
}

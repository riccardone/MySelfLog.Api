using System;
using Finbuckle.MultiTenant;
using Microsoft.Extensions.Logging;
using MySelfLog.Admin.Model;
using MySelfLog.Contracts.Api;
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
            IMessageSender sender = null;

            if (_store == null)
                throw new Exception("Store is null");

            var tenant = _store.TryGetByIdentifierAsync(source).Result;
            if (tenant == null)
                throw new Exception($"I can't find a configured tenant with source: '{source}'");
            if (!string.IsNullOrWhiteSpace(tenant.MessageBusLink))
            {
                _logger.LogInformation($"Configuring '{nameof(MessageSenderToEventStore)}'");
                sender = new MessageSenderToEventStore();
            }
            else
            {
                _logger.LogWarning($"Configuring '{nameof(MessageSenderInMemory)}'. Please use this only for testing!");
                sender = new MessageSenderInMemory();
            }

            if (sender == null)
                throw new Exception("Review configuration settings as I can't build a message sender");

            return sender;
        }
    }
}

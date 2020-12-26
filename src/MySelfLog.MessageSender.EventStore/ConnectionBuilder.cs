using System;
using EventStore.ClientAPI;
using MySelfLog.Contracts.Api;

namespace MySelfLog.MessageSender.EventStore
{
    public class ConnectionBuilder : IConnectionBuilder
    {
        public Uri ConnectionString { get; }
        public ConnectionSettings ConnectionSettings { get; }
        public string ConnectionName { get; }

        public IEventStoreConnection Build()
        {
            return EventStoreConnection.Create(ConnectionSettings, ConnectionString, ConnectionName);
        }

        public ConnectionBuilder(Uri connectionString, ConnectionSettings connectionSettings, string connectionName)
        {
            ConnectionString = connectionString;
            ConnectionSettings = connectionSettings;
            ConnectionName = connectionName;
        }

        public ConnectionBuilder(IBusSettings settings)
        {
            ConnectionString = new Uri(settings.Link);
            ConnectionSettings = global::EventStore.ClientAPI.ConnectionSettings.Default;
            ConnectionName = $"{settings.Name}-{Guid.NewGuid()}";
        }
    }
}

using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace MySelfLog.MessageSender.EventStore
{
    public interface IConnectionBuilder
    {
        IEventStoreConnection Build();
    }
}

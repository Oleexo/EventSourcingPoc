using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;
using Microsoft.Azure.ServiceBus;

namespace EventSourcing.Poc.EventSourcing.Event {
    public interface IEventQueue {
        Task Send(IEventWrapper eventWrapped);
        void RegisterMessageHandler(Func<IEventWrapper, CancellationToken, Task> handler);
        void RegisterMessageHandler(Func<IEventWrapper, CancellationToken, Task> handler, RegisterHandlerOptions handlerOptions);
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;

namespace EventSourcing.Poc.EventSourcing.Event {
    public interface IEventQueue {
        Task Send(IEventWrapper eventWrapped);
        void RegisterMessageHandler(Func<IEventWrapper, CancellationToken, Task> handler);
    }
}
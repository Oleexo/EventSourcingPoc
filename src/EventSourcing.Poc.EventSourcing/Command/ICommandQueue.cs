using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;
using Microsoft.Azure.ServiceBus;

namespace EventSourcing.Poc.EventSourcing.Command {
    public interface ICommandQueue {
        Task Send(ICommandWrapper commandWrapper);

        void RegisterMessageHandler(Func<ICommandWrapper, CancellationToken, Task> handler, RegisterHandlerOptions handlerOptions);
    }
}
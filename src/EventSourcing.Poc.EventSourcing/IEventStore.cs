using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing {
    public interface IEventStore {
        Task Save(IEventWrapper wrappedEvent);
        Task Save(IReadOnlyCollection<IEventWrapper> wrappedEvents);
    }
}
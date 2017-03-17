using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;

namespace EventSourcing.Poc.EventSourcing {
    public interface IEventStore {
        Task Save(IEventWrapper wrappedEvent);
        Task Save(IReadOnlyCollection<IEventWrapper> wrappedEvents);
    }
}
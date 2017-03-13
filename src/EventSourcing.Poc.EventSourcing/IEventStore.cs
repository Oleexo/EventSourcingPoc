using System.Collections.Generic;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing {
    public interface IEventStore {
        void Save(IEvent @event);
        void Save(IReadOnlyCollection<IEvent> events);
    }
}
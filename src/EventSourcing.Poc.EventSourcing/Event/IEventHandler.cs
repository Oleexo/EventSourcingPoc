using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Event {
    public interface IEventHandler<in TEvent> : IHandler where TEvent : IEvent {
        Task<IReadOnlyCollection<IAction>> Handle(TEvent @event);
    }
}
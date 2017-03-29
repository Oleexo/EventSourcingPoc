using EventSourcing.Poc.EventSourcing.Event;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.Processing {
    public interface IEventHandlerFactory {
        IEventHandler<TEvent> Resolve<TEvent>() where TEvent : IEvent;
    }
}
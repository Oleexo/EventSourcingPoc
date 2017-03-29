using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Event {
    public interface IEventProcessor {
        Task Process<TEvent>(IEventWrapper<TEvent> eventWrapper) where TEvent : IEvent;
    }
}
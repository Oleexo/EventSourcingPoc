using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.Messages;
using JetBrains.Annotations;

namespace EventSourcing.Poc.EventSourcing.Event {
    public interface IEventDispatcher {
        Task SendAsync<TEvent>([NotNull] TEvent @event) where TEvent : IEvent;
        Task SendAsync<TEvent>([NotNull] IReadOnlyCollection<TEvent> events) where TEvent : IEvent;
    }
}
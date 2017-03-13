using System;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public class EventWrapper<TEvent> : IEventWrapper<TEvent> where TEvent : IEvent {
        public EventWrapper(TEvent @event, ICommand parent) {
            Id = Guid.NewGuid();
            Event = @event;
        }

        public Guid Id { get; set; }
        public TEvent Event { get; set; }
        public Guid ParentId { get; set; }
    }
}
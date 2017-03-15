using System;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public class EventWrapper<TEvent> : IEventWrapper<TEvent> where TEvent : IEvent {
        public EventWrapper(TEvent @event, ICommandWrapper parent) {
            Id = Guid.NewGuid();
            Event = @event;
            ParentId = parent.Id;
        }

        public Guid Id { get; set; }
        public bool IsLinkToJob => JobId.HasValue;
        public TEvent Event { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? JobId { get; set; }
    }
}
using System;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public interface IEventWrapper : IWrapper {
        Guid? ParentId { get; set; }
        Guid? JobId { get; set; }
    }
    public interface IEventWrapper<TEvent> : IEventWrapper where TEvent : IEvent {
        TEvent Event { get; set; }
    }
}
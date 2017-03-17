using System;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public interface IEventWrapper : IWrapper {
        Guid? ParentId { get; set; }
        Guid? JobId { get; set; }
        IEvent Event { get; }
    }

    public interface IEventWrapper<TEvent> : IEventWrapper where TEvent : IEvent {
        new TEvent Event { get; set; }
    }
}
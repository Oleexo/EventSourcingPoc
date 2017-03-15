using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Event;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventProcessing {
    public class EventDispatcher : IEventDispatcher {
        private readonly IEventQueue _eventQueue;
        private readonly IEventStore _eventStore;
        private readonly IJobHandler _jobHandler;

        public EventDispatcher(IEventStore eventStore, 
            IJobHandler jobHandler, 
            IEventQueue eventQueue) {
            _eventStore = eventStore;
            _jobHandler = jobHandler;
            _eventQueue = eventQueue;
        }

        public async Task Send<TCommand, TEvent>(ICommandWrapper<TCommand> parentCommand, TEvent @event)
            where TCommand : ICommand where TEvent : IEvent {
            await Send<TCommand, TEvent>(parentCommand, new[] {@event});
        }

        public async Task Send<TCommand, TEvent>(ICommandWrapper<TCommand> parentCommand,
            IReadOnlyCollection<TEvent> events) where TCommand : ICommand where TEvent : IEvent {
            var wrappedEvents = events
                .Select(e => e.Wrap(parentCommand))
                .ToArray();
            await _eventStore.Save(wrappedEvents);
            if (parentCommand.IsLinkToJob) {
                await _jobHandler.Associate(parentCommand, wrappedEvents);
            }
            foreach (var wrappedEvent in wrappedEvents) {
                await _eventQueue.Send(wrappedEvent);
            }
        }

        public Task Send<TEvent>(TEvent @event) where TEvent : IEvent {
            throw new NotImplementedException();
        }

        public Task Send<TEvent>(IReadOnlyCollection<TEvent> events) where TEvent : IEvent {
            throw new NotImplementedException();
        }
    }
}
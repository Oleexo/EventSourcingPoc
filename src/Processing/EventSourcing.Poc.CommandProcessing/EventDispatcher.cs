using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Event;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.Processing {
    public class EventDispatcher : IEventDispatcher {
        private readonly IEventQueue _eventQueue;
        private readonly IEventStore _eventStore;
        private readonly IJobHandler _jobHandler;
        private readonly IEventProcessor _eventProcessor;

        public EventDispatcher(IEventStore eventStore,
            IJobHandler jobHandler,
            IEventProcessor eventProcessor) {
            _eventStore = eventStore;
            _jobHandler = jobHandler;
            _eventProcessor = eventProcessor;
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
            if (parentCommand.IsLinkToJob) {
                await _jobHandler.Associate(parentCommand, wrappedEvents);
            }
            await _eventStore.Save(wrappedEvents);
            await ProcessEvents(wrappedEvents);
        }

        private async Task ProcessEvents(IEnumerable<IEventWrapper> wrappedEvents) {
            var methodInfo = _eventProcessor.GetType().GetMethod("Process");
            //Parallel.ForEach(wrappedEvents, async wrappedEvent => {
            //    var wrappedEventType = wrappedEvent.GetType().GetTypeInfo().GetGenericArguments()[0];
            //    await (Task)methodInfo.MakeGenericMethod(wrappedEventType)
            //        .Invoke(_eventProcessor, new object[] { wrappedEvent });
            //});
            foreach (var wrappedEvent in wrappedEvents) {
                var wrappedEventType = wrappedEvent.GetType().GetTypeInfo().GenericTypeArguments[0];
                await (Task)methodInfo.MakeGenericMethod(wrappedEventType)
                    .Invoke(_eventProcessor, new object[] { wrappedEvent });
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
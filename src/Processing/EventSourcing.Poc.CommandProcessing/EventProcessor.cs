using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.Processing {
    public class EventProcessor {
        private readonly EventHandlerFactory _eventHandlerFactory;
        private readonly IJobHandler _jobHandler;

        public EventProcessor(EventHandlerFactory eventHandlerFactory, IJobHandler jobHandler) {
            _eventHandlerFactory = eventHandlerFactory;
            _jobHandler = jobHandler;
        }

        public async Task Process<TEvent>(IEventWrapper<TEvent> eventWrapper) where TEvent : IEvent {
            var eventHandler = _eventHandlerFactory.Resolve<TEvent>();
            IReadOnlyCollection<IAction> actions;
            try {
                actions = await eventHandler.Handle(eventWrapper.Event);
            }
            catch (Exception ex) {
                if (eventWrapper.IsLinkToJob) {
                    // todo
                }
                return;
            }
        }
    }
}
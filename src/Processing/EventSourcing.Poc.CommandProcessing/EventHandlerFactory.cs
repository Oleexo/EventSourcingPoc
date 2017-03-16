using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Event;
using EventSourcing.Poc.EventSourcing.Exceptions;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.Processing {
    public class EventHandlerFactory {
        private static IReadOnlyDictionary<Type, Type> _eventToEventHandlers;
        private readonly IServiceProvider _serviceProvider;

        static EventHandlerFactory()
        {
            _eventToEventHandlers = new Dictionary<Type, Type>();
        }

        public EventHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static void AddEventHandler(Assembly assembly)
        {
            var newMapping =
                HandlerFactoryHelper.CreateHandler<EventHandlerAttribute>(assembly, typeof(IEventHandler<>));
            _eventToEventHandlers = _eventToEventHandlers.Concat(newMapping)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public IEventHandler<TEvent> Resolve<TEvent>() where TEvent : IEvent
        {
            if (_eventToEventHandlers.ContainsKey(typeof(TEvent)))
            {
                var eventHandler = _eventToEventHandlers[typeof(TEvent)];
                return _serviceProvider.GetService(eventHandler) as IEventHandler<TEvent>;
            }
            throw new NoEventHandlerRegisteredException();
        }
    }
}
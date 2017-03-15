using System;
using System.Reflection;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public static class WrapperExtensions {
        public static ICommandWrapper Wrap<TCommand>(this TCommand command) where TCommand : ICommand {
            var commandWrapperGeneric = typeof(CommandWrapper<>).GetTypeInfo().MakeGenericType(command.GetType());
            return Activator.CreateInstance(commandWrapperGeneric, command) as ICommandWrapper;
        }

        public static IEventWrapper Wrap<TEvent>(this TEvent @event, ICommandWrapper parent) where TEvent : IEvent {
            var eventWrapperGeneric = typeof(EventWrapper<>).GetTypeInfo().MakeGenericType(@event.GetType());
            return Activator.CreateInstance(eventWrapperGeneric, @event, parent) as IEventWrapper;
        }
    }
}
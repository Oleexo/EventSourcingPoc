using System;
using System.Reflection;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public static class WrapperExtensions {
        public static ICommandWrapper Wrap(this ICommand command) {
            var commandWrapperGeneric = typeof(CommandWrapper<>)
                .GetTypeInfo()
                .MakeGenericType(command.GetType());
            return Activator.CreateInstance(commandWrapperGeneric, command) as ICommandWrapper;
        }

        public static IActionWrapper Wrap(this IAction action) {
            var actionWrapperGeneric = typeof(ActionWrapper<>)
                .GetTypeInfo()
                .MakeGenericType(action.GetType());
            return Activator.CreateInstance(actionWrapperGeneric, action) as IActionWrapper;
        }

        public static IEventWrapper Wrap(this IEvent @event, ICommandWrapper parent) {
            var eventWrapperGeneric = typeof(EventWrapper<>)
                .GetTypeInfo()
                .MakeGenericType(@event.GetType());
            return Activator.CreateInstance(eventWrapperGeneric, @event, parent) as IEventWrapper;
        }
    }
}
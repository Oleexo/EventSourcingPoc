using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public static class WrapperExtensions {
        public static ICommandWrapper<TCommand> Wrap<TCommand>(this TCommand command) where TCommand : ICommand {
            return new CommandWrapper<TCommand>(command);
        }

        public static IEventWrapper<TEvent> Wrap<TEvent>(this TEvent @event, ICommand parent) where TEvent : IEvent {
            return new EventWrapper<TEvent>(@event, parent);
        }
    }
}
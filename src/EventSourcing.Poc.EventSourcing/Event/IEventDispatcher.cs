using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;
using JetBrains.Annotations;

namespace EventSourcing.Poc.EventSourcing.Event {
    public interface IEventDispatcher {
        Task Send<TCommand, TEvent>([NotNull] ICommandWrapper<TCommand> parentCommand, TEvent @event) where TEvent : IEvent where TCommand: ICommand;
        Task Send<TCommand, TEvent>([NotNull] ICommandWrapper<TCommand> parentCommand, IReadOnlyCollection<TEvent> events) where TEvent : IEvent where TCommand : ICommand;
        Task Send<TEvent>([NotNull] TEvent @event) where TEvent : IEvent;
        Task Send<TEvent>([NotNull] IReadOnlyCollection<TEvent> events) where TEvent : IEvent;
    }
}
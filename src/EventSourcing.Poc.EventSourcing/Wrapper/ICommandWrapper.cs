using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public interface ICommandWrapper : IWrapper {
    }

    public interface ICommandWrapper<TCommand> : ICommandWrapper where TCommand : ICommand {
        TCommand Command { get; set; }
    }
}
using System;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public class CommandWrapper<TCommand> : ICommandWrapper<TCommand> where TCommand : ICommand {
        public CommandWrapper(TCommand command) {
            Id = Guid.NewGuid();
            Command = command;
        }

        public CommandWrapper() {
        }

        public Guid Id { get; set; }
        public TCommand Command { get; set; }
    }
}
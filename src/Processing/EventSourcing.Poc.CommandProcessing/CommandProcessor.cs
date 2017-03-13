using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;
using Microsoft.Azure.Amqp;

namespace EventSourcing.Poc.CommandProcessing {
    public class CommandProcessor {
        private readonly CommandHandlerFactory _commandHandlerFactory;

        public CommandProcessor(CommandHandlerFactory commandHandlerFactory) {
            _commandHandlerFactory = commandHandlerFactory;
        }

        public Task Process<TCommand>(ICommandWrapper<TCommand> commandWrapper) where TCommand : ICommand {
            var commandHandler = _commandHandlerFactory.Resolve<TCommand>();
            var events = commandHandler.Handle(commandWrapper.Command);
            return Task.CompletedTask;
        }
    }
}
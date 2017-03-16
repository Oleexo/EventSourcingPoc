using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Event;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.Processing {
    public class CommandProcessor {
        private readonly CommandHandlerFactory _commandHandlerFactory;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IJobHandler _jobHandler;

        public CommandProcessor(CommandHandlerFactory commandHandlerFactory,
            IEventDispatcher eventDispatcher,
            IJobHandler jobHandler) {
            _commandHandlerFactory = commandHandlerFactory;
            _eventDispatcher = eventDispatcher;
            _jobHandler = jobHandler;
        }

        public async Task Process<TCommand>(ICommandWrapper<TCommand> commandWrapper) where TCommand : ICommand {
            var commandHandler = _commandHandlerFactory.Resolve<TCommand>();
            IReadOnlyCollection<IEvent> events;
            try {
                events = await commandHandler.Handle(commandWrapper.Command);
            }
            catch (Exception ex) {
                if (commandWrapper.IsLinkToJob) {
                    await _jobHandler.Fail(commandWrapper, ex);
                }
                return;
            }
            if (events.Any()) {
                await _eventDispatcher.Send(commandWrapper, events);
            }
            else {
                await _jobHandler.Done(commandWrapper);
            }
        }
    }
}
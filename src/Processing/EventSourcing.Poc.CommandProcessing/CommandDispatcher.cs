using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Job;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.CommandProcessing
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly ICommandStore _commandStore;
        private readonly ICommandQueue _commandQueue;
        private readonly IJobFactory _jobFactory;

        public CommandDispatcher(ICommandStore commandStore, ICommandQueue commandQueue, IJobFactory jobFactory) {
            _commandStore = commandStore;
            _commandQueue = commandQueue;
            _jobFactory = jobFactory;
        }

        public async Task<IJob> Send<TCommand>(TCommand command, TimeSpan? timeout = null) where TCommand : ICommand {
            var wrappedCommand = command.Wrap();
            await _commandStore.Save(wrappedCommand);
            await SendToQueue(wrappedCommand);
            return _jobFactory.Create(wrappedCommand);
        }

        public Task<IJob> Send<TCommand>(IReadOnlyCollection<TCommand> commands, TimeSpan? timeout = null) where TCommand : ICommand {
            throw new NotImplementedException();
        }

        private async Task SendToQueue(ICommandWrapper commandWrapper) {
            await _commandQueue.Send(commandWrapper);
        }
    }
}

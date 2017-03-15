using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.Processing
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
            var job = await _jobFactory.Create(wrappedCommand);
            await _commandStore.Save(wrappedCommand);
            await SendToQueue(wrappedCommand);
            return job;
        }

        public async Task<IJob> Send<TCommand>(IReadOnlyCollection<TCommand> commands, TimeSpan? timeout = null) where TCommand : ICommand {
            var wrappedCommands = commands.Select(c => c.Wrap()).ToArray();
            var job = await _jobFactory.Create(wrappedCommands);
            await _commandStore.Save(wrappedCommands);
            foreach (var wrappedCommand in wrappedCommands) {
                await SendToQueue(wrappedCommand);
            }
            return job;
        }

        private async Task SendToQueue(ICommandWrapper commandWrapper) {
            await _commandQueue.Send(commandWrapper);
        }
    }
}

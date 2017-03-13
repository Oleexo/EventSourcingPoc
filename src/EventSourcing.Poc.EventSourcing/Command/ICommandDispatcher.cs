using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Job;
using EventSourcing.Poc.Messages;
using JetBrains.Annotations;

namespace EventSourcing.Poc.EventSourcing.Command {
    public interface ICommandDispatcher {
        Task<IJob> Send<TCommand>([NotNull] TCommand command, [CanBeNull] TimeSpan? timeout = null)
            where TCommand : ICommand;

        Task<IJob> Send<TCommand>([NotNull] IReadOnlyCollection<TCommand> commands,
            [CanBeNull]TimeSpan? timeout = null) where TCommand : ICommand;
    }
}
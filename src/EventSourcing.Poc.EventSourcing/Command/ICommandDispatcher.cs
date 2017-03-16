using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;
using JetBrains.Annotations;

namespace EventSourcing.Poc.EventSourcing.Command {
    public interface ICommandDispatcher {
        Task<IJob> Send([NotNull] ICommand command, [CanBeNull] TimeSpan? timeout = null);

        Task<IJob> Send([NotNull] IReadOnlyCollection<ICommand> commands,
            [CanBeNull] TimeSpan? timeout = null);
    }

    public interface IActionDispatcher {
        Task Send([NotNull]IEventWrapper eventParent, [NotNull] IAction action);
        Task Send([NotNull]IEventWrapper eventParent, [NotNull] IReadOnlyCollection<IAction> actions);
    }
}
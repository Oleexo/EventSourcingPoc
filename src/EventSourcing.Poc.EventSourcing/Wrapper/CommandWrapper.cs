using System;
using EventSourcing.Poc.EventSourcing.Jobs;
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
        public bool IsLinkToJob => JobId.HasValue;
        public Guid? JobId { get; set; }
        public void LinkToJob(IJob job) {
            JobId = job.Id;
        }

        public TCommand Command { get; set; }
    }

    public class ActionWrapper<TCommand> : CommandWrapper<TCommand>, IActionWrapper<TCommand> 
        where TCommand : IAction {
        public ActionWrapper(TCommand command) : base(command) {
        }

        public ActionWrapper() {
        }
    }
}
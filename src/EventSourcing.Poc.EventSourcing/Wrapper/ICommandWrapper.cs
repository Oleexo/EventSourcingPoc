using System;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public interface ICommandWrapper : IWrapper {
        Guid? JobId { get; set; }
        ICommand Command { get; }
        void LinkToJob(IJob job);
    }

    public interface ICommandWrapper<TCommand> : ICommandWrapper where TCommand : ICommand {
        new TCommand Command { get; set; }
    }
}
using System;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public interface ICommandWrapper : IWrapper {
        Guid? JobId { get; set; }
        void LinkToJob(IJob job);
        ICommand Command { get; }
    }

    public interface ICommandWrapper<TCommand> : ICommandWrapper where TCommand : ICommand {
        new TCommand Command { get; set; }
    }


}
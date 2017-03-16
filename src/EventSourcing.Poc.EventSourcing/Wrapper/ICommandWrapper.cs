using System;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public interface ICommandWrapper : IWrapper {
        Guid? JobId { get; set; }
        void LinkToJob(IJob job);
    }

    public interface ICommandWrapper<TCommand> : ICommandWrapper where TCommand : ICommand {
        TCommand Command { get; set; }
    }

    public interface IActionWrapper : ICommandWrapper {
        
    }

    public interface IActionWrapper<TCommand> : IActionWrapper, ICommandWrapper<TCommand> where TCommand : IAction {
        
    }

}
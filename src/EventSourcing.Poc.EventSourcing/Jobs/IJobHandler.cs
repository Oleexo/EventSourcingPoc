using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;

namespace EventSourcing.Poc.EventSourcing.Jobs {
    public interface IJobHandler {
        Task Initialize(IJob job, ICommandWrapper wrappedCommand);
        Task Initialize(IJob job, IReadOnlyCollection<ICommandWrapper> wrappedCommands);
        Task Fail(ICommandWrapper commandWrapper, Exception exception);
        Task Fail(IEventWrapper eventWrapper, Exception exception);
        Task Associate(ICommandWrapper commandParent, IReadOnlyCollection<IEventWrapper> eventWrappers);
        Task Associate(IEventWrapper eventParent, IReadOnlyCollection<IActionWrapper> wrappedActions);
        Task Done(ICommandWrapper commandWrapper);
        Task Done(IEventWrapper eventWrapper);
    }

    public interface IJobFollower {
        Task<IJob> GetInformation(string jobId);
    }
}
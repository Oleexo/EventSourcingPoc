using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;

namespace EventSourcing.Poc.EventSourcing.Jobs {
    public interface IJobHandler {
        Task Initialize(IJob job, ICommandWrapper wrappedCommand);
        Task Initialize(IJob job, IReadOnlyCollection<ICommandWrapper> wrappedCommands);
        Task Fail(ICommandWrapper commandWrapper, Exception exception);
        Task Associate(ICommandWrapper commandWrapper, IReadOnlyCollection<IEventWrapper> eventWrappers);
        Task<IJob> GetInformation(string jobId);
    }
}
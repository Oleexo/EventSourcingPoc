using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;

namespace EventSourcing.Poc.EventSourcing.Jobs {
    public interface IJobFactory {
        Task<IJob> Create(ICommandWrapper wrappedCommand);
        Task<IJob> Create(IReadOnlyCollection<ICommandWrapper> wrappedCommands);
    }
}
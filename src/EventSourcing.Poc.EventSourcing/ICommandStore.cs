using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Wrapper;

namespace EventSourcing.Poc.EventSourcing {
    public interface ICommandStore {
        Task Save(ICommandWrapper commandWrapped);
        Task Save(IReadOnlyCollection<ICommandWrapper> commandWrappeds);
    }
}
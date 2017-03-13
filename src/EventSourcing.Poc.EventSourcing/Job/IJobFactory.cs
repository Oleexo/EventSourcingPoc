using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Job {
    public interface IJobFactory {
        IJob Create(ICommandWrapper wrappedCommand);
    }

    public class JobFactory : IJobFactory {
        public IJob Create(ICommandWrapper wrappedCommand) {
            throw new System.NotImplementedException();
        }
    }
}
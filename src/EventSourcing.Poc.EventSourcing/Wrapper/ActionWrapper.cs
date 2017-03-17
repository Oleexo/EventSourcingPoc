using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public class ActionWrapper<TCommand> : CommandWrapper<TCommand>, IActionWrapper<TCommand> 
        where TCommand : IAction {
        public ActionWrapper(TCommand command) : base(command) {
        }

        public ActionWrapper() {
        }
    }
}
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public interface IActionWrapper : ICommandWrapper {
        
    }


    public interface IActionWrapper<TCommand> : IActionWrapper, ICommandWrapper<TCommand> where TCommand : IAction
    {

    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.EventSourcing.Command {
    public interface ICommandHandler<in TCommand> where TCommand : ICommand {
        Task<IReadOnlyCollection<IEvent>> Handle(TCommand command);
    }
}
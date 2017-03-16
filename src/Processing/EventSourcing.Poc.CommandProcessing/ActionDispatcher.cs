using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.Processing {
    public class ActionDispatcher : IActionDispatcher {
        private readonly ICommandQueue _commandQueue;
        private readonly ICommandStore _commandStore;
        private readonly IJobHandler _jobHandler;

        public ActionDispatcher(ICommandStore commandStore, ICommandQueue commandQueue, IJobHandler jobHandler) {
            _commandStore = commandStore;
            _commandQueue = commandQueue;
            _jobHandler = jobHandler;
        }

        public async Task Send(IEventWrapper eventParent, IAction action) {
            await Send(eventParent, new[] {action});
        }

        public async Task Send(IEventWrapper eventParent, IReadOnlyCollection<IAction> actions) {
            var wrappedActions = actions
                .Select(c => c.Wrap())
                .ToArray();
            if (eventParent.IsLinkToJob) {
                await _jobHandler.Associate(eventParent, wrappedActions);
            }
            await _commandStore.Save(wrappedActions);
            foreach (var wrappedAction in wrappedActions) {
                await _commandQueue.Send(wrappedAction);
            }
        }
    }
}
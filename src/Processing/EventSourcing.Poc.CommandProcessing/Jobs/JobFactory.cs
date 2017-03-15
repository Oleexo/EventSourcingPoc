using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;

namespace EventSourcing.Poc.Processing.Jobs {
    public class JobFactory : IJobFactory {
        private readonly IJobHandler _jobHandler;

        public JobFactory(IJobHandler jobHandler) {
            _jobHandler = jobHandler;
        }

        public async Task<IJob> Create(ICommandWrapper wrappedCommand) {
            return await Create(new[] {wrappedCommand});
        }

        public async Task<IJob> Create(IReadOnlyCollection<ICommandWrapper> wrappedCommands) {
            var job = new Job {
                Id = Guid.NewGuid(),
                CommandIdentifiers = wrappedCommands
                    .Select(wc => wc.Id)
                    .ToList()
            };
            foreach (var wrappedCommand in wrappedCommands) {
                wrappedCommand.LinkToJob(job);
            }
            await _jobHandler.Initialize(job, wrappedCommands);
            return job;
        }
    }
}
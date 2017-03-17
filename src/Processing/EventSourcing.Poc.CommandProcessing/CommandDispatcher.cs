using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.Processing {
    public class CommandDispatcher : ICommandDispatcher {
        private readonly ICommandQueue _commandQueue;
        private readonly ICommandStore _commandStore;
        private readonly IJobFactory _jobFactory;
        private readonly IJobFollower _jobFollower;


        public CommandDispatcher(ICommandStore commandStore,
            ICommandQueue commandQueue,
            IJobFactory jobFactory,
            IJobFollower jobFollower) {
            _commandStore = commandStore;
            _commandQueue = commandQueue;
            _jobFactory = jobFactory;
            _jobFollower = jobFollower;
        }

        public Task<IJob> Send(ICommand command, TimeSpan? timeout = null) {
            return Send(new[] {command}, timeout);
        }

        public async Task<IJob> Send(IReadOnlyCollection<ICommand> commands, TimeSpan? timeout = null) {
            var wrappedCommands = commands.Select(c => c.Wrap()).ToArray();
            var job = await _jobFactory.Create(wrappedCommands);
            await _commandStore.Save(wrappedCommands);
            foreach (var wrappedCommand in wrappedCommands) {
                await _commandQueue.Send(wrappedCommand);
            }
            if (!timeout.HasValue) {
                return job;
            }
            return TryWait(job, timeout.Value) ?? job;
        }

        private IJob TryWait(IJob job, TimeSpan timeout) {
            var duration = (long) timeout.TotalMilliseconds;
            var waitTime = (int) (duration / 10);
            if (waitTime < 1000) {
                waitTime = 1000;
            }
            try {
                var task = Task.Run(async () => {
                    var watch = new Stopwatch();
                    long elapsedTime = 0;
                    var currentWaitTime = waitTime;
                    do {
                        watch.Start();
                        if (currentWaitTime > 0) {
                            await Task.Delay(currentWaitTime);
                        }
                        var jobInformation = await _jobFollower.GetInformation(job.Id.ToString());
                        watch.Stop();
                        elapsedTime += watch.ElapsedMilliseconds;
                        if (jobInformation.IsDone) {
                            return jobInformation;
                        }
                    } while (elapsedTime < duration);
                    return null;
                });
                task.Wait(timeout);
                return task.Result;
            }
            catch (Exception e) {
                return null;
            }
        }
    }
}
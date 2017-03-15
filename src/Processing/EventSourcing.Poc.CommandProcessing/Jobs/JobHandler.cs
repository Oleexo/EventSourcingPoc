using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace EventSourcing.Poc.Processing.Jobs {
    public class JobHandler : IJobHandler {
        private readonly CloudTable _actionTable;
        private readonly CloudTable _commandTable;
        private readonly CloudTable _eventTable;
        private readonly CloudTable _jobTable;
        private readonly IJsonConverter _jsonConverter;

        public JobHandler(IOptions<JobHandlerOptions> options, IJsonConverter jsonConverter) {
            _jsonConverter = jsonConverter;
            var cloudTableClient = CloudStorageAccount.Parse(options.Value.ConnectionString)
                .CreateCloudTableClient();
            _jobTable = cloudTableClient.GetTableReference(options.Value.JobTableName);
            _commandTable = cloudTableClient.GetTableReference(options.Value.CommandTableName);
            _eventTable = cloudTableClient.GetTableReference(options.Value.EventTableName);
            _actionTable = cloudTableClient.GetTableReference(options.Value.ActionTableName);
            _jobTable.CreateIfNotExistsAsync().Wait();
            _commandTable.CreateIfNotExistsAsync().Wait();
            _eventTable.CreateIfNotExistsAsync().Wait();
            _actionTable.CreateIfNotExistsAsync().Wait();
        }

        public async Task Initialize(IJob job, ICommandWrapper wrappedCommand) {
            await Initialize(job, new[] {wrappedCommand});
        }

        public async Task Initialize(IJob job, IReadOnlyCollection<ICommandWrapper> wrappedCommands) {
            var jobRow = new JobRow {
                PartitionKey = "Job",
                RowKey = job.Id.ToString(),
                Timestamp = DateTimeOffset.UtcNow,
                IsStarted = job.IsStarted,
                IsDone = job.IsDone,
                Commands = _jsonConverter.Serialize(job.CommandIdentifiers)
            };
            await _jobTable.ExecuteAsync(TableOperation.Insert(jobRow));
            var batchOperation = new TableBatchOperation();
            var commandRows = wrappedCommands.Select(wc => new CommandRow {
                PartitionKey = "Command",
                RowKey = wc.Id.ToString(),
                IsStarted = job.IsStarted,
                IsDone = job.IsDone
            }).ToArray();
            foreach (var commandRow in commandRows) {
                batchOperation.Add(TableOperation.Insert(commandRow));
            }
            await _commandTable.ExecuteBatchAsync(batchOperation);
        }

        public async Task Fail(ICommandWrapper commandWrapper, Exception exception) {
            var retrieveOperation = TableOperation.Retrieve<CommandRow>("Command", commandWrapper.Id.ToString());
            var tableResult = await _commandTable.ExecuteAsync(retrieveOperation);
            var commandRow = tableResult.Result as CommandRow;
            if (commandRow == null) {
                throw new Exception();
            }
            commandRow.IsStarted = true;
            commandRow.IsDone = true;
            commandRow.ExecutionDate = DateTimeOffset.UtcNow;
            commandRow.IsSuccessful = false;
            var replaceOperation = TableOperation.Replace(commandRow);
            await _commandTable.ExecuteAsync(replaceOperation);
        }

        public async Task Associate(ICommandWrapper commandWrapper, IReadOnlyCollection<IEventWrapper> eventWrappers) {
            var retrieveOperation = TableOperation.Retrieve<CommandRow>("Command", commandWrapper.Id.ToString());
            var tableResult = await _commandTable.ExecuteAsync(retrieveOperation);
            var commandRow = tableResult.Result as CommandRow;
            if (commandRow == null)
            {
                throw new Exception();
            }
            commandRow.IsStarted = true;
            commandRow.ExecutionDate =DateTimeOffset.UtcNow;
            if (eventWrappers.Any()) {
                commandRow.Events = _jsonConverter.Serialize(eventWrappers.Select(e => e.Id));
            }
            else {
                commandRow.IsDone = true;
                commandRow.IsSuccessful = true;
            }
            var replaceOperation = TableOperation.Replace(commandRow);
            await _commandTable.ExecuteAsync(replaceOperation);
            var batchOperation = new TableBatchOperation();
            foreach (var eventWrapper in eventWrappers) {
                eventWrapper.JobId = commandWrapper.JobId;
                batchOperation.Add(TableOperation.Insert(new EventRow {
                    RowKey = eventWrapper.Id.ToString(),
                    PartitionKey = "Event",
                    ParentId = eventWrapper.ParentId,
                    Timestamp = DateTimeOffset.UtcNow,
                    IsDone = false,
                    IsStarted = false,
                    ExecutionDate = null,
                    IsSuccessful = false
                }));
            }
            await _eventTable.ExecuteBatchAsync(batchOperation);
        }

        public Task<IJob> GetInformation(string jobId) {
            throw new NotImplementedException();
        }

        #region Private classes

        private class JobRow : TableEntity {
            public bool IsStarted { get; set; }
            public bool IsDone { get; set; }
            public string Commands { get; set; }
        }

        private class CommandRow : TableEntity {
            public bool IsStarted { get; set; }
            public bool IsDone { get; set; }
            public DateTimeOffset? ExecutionDate { get; set; }
            public bool IsSuccessful { get; set; }
            public string Events { get; set; }
        }

        private class EventRow : TableEntity {
            public bool IsStarted { get; set; }
            public bool IsDone { get; set; }
            public DateTimeOffset? ExecutionDate { get; set; }
            public bool IsSuccessful { get; set; }
            public Guid? ParentId { get; set; }
        }

        private class ActionRow : TableEntity {
        }

        #endregion
    }
}
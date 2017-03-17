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
    public class JobHandler : IJobHandler, IJobFollower {
        private readonly CloudTable _commandTable;
        private readonly CloudTable _eventTable;
        private readonly JobArchive _jobArchive;
        private readonly CloudTable _jobTable;
        private readonly IJsonConverter _jsonConverter;

        public JobHandler(IOptions<JobHandlerOptions> options, IJsonConverter jsonConverter) {
            _jsonConverter = jsonConverter;
            var cloudTableClient = CloudStorageAccount.Parse(options.Value.ConnectionString)
                .CreateCloudTableClient();
            _jobTable = cloudTableClient.GetTableReference(options.Value.JobTableName);
            _commandTable = cloudTableClient.GetTableReference(options.Value.CommandTableName);
            _eventTable = cloudTableClient.GetTableReference(options.Value.EventTableName);
            _jobTable.CreateIfNotExistsAsync().Wait();
            _commandTable.CreateIfNotExistsAsync().Wait();
            _eventTable.CreateIfNotExistsAsync().Wait();
            _jobArchive = new JobArchive(options.Value.ConnectionString, options.Value.ArchiveStorageName,
                options.Value.ArchiveTableName, jsonConverter);
        }


        public async Task<IJob> GetInformation(string jobId) {
            var jobArchived = await _jobArchive.Get(Guid.Parse(jobId));
            if (jobArchived != null) {
                return jobArchived;
            }
            var jobRow = await Retrieve<JobRow>(jobId);
            var job = await From(jobRow);
            if (!job.CheckAllDependenciesDone()) {
                return job;
            }
            job.IsDone = true;
            await _jobArchive.Archive(job);
            await CleanupTables(job);
            return job;
        }

        public async Task Initialize(IJob job, ICommandWrapper wrappedCommand) {
            await Initialize(job, new[] {wrappedCommand});
        }

        public async Task Initialize(IJob job, IReadOnlyCollection<ICommandWrapper> wrappedCommands) {
            var jobRow = new JobRow {
                PartitionKey = "Job",
                RowKey = job.Id.ToString(),
                Timestamp = DateTimeOffset.UtcNow,
                IsDone = job.IsDone,
                CommandIdentifiers = _jsonConverter.Serialize(job.CommandInformations.Select(ci => ci.Id))
            };
            await _jobTable.ExecuteAsync(TableOperation.Insert(jobRow));
            var batchOperation = new TableBatchOperation();
            var commandRows = wrappedCommands.Select(wc => new CommandRow {
                    Type = wc.Command.GetType().FullName,
                    PartitionKey = "Command",
                    RowKey = wc.Id.ToString(),
                    IsDone = job.IsDone
                })
                .ToArray();
            foreach (var commandRow in commandRows) {
                batchOperation.Add(TableOperation.Insert(commandRow));
            }
            await _commandTable.ExecuteBatchAsync(batchOperation);
        }

        public async Task Fail(ICommandWrapper commandWrapper, Exception exception) {
            var commandRow = await Retrieve<CommandRow>(commandWrapper.Id.ToString());
            commandRow.IsStarted = true;
            commandRow.IsDone = true;
            commandRow.ExecutionDate = DateTimeOffset.UtcNow;
            commandRow.IsSuccessful = false;
            var replaceOperation = TableOperation.Replace(commandRow);
            await _commandTable.ExecuteAsync(replaceOperation);
        }

        public async Task Fail(IEventWrapper eventWrapper, Exception exception) {
            var eventRow = await Retrieve<EventRow>(eventWrapper.Id.ToString());
            eventRow.IsStarted = true;
            eventRow.IsDone = true;
            eventRow.ExecutionDate = DateTimeOffset.UtcNow;
            eventRow.IsSuccessful = false;
            var replaceOperation = TableOperation.Replace(eventRow);
            await _eventTable.ExecuteAsync(replaceOperation);
        }

        public async Task Associate(ICommandWrapper commandParent, IReadOnlyCollection<IEventWrapper> eventWrappers) {
            var commandRow = await Retrieve<CommandRow>(commandParent.Id.ToString());
            commandRow.IsStarted = true;
            commandRow.ExecutionDate = DateTimeOffset.UtcNow;
            commandRow.IsDone = true;
            commandRow.IsSuccessful = true;
            if (eventWrappers.Any()) {
                commandRow.Events = _jsonConverter.Serialize(eventWrappers.Select(e => e.Id));
            }
            var replaceOperation = TableOperation.Replace(commandRow);
            await _commandTable.ExecuteAsync(replaceOperation);
            if (!eventWrappers.Any()) {
                return;
            }
            var batchOperation = new TableBatchOperation();
            foreach (var eventWrapper in eventWrappers) {
                eventWrapper.JobId = commandParent.JobId;
                batchOperation.Add(TableOperation.Insert(new EventRow {
                    Type = eventWrapper.Event.GetType().FullName,
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

        public async Task Associate(IEventWrapper eventParent, IReadOnlyCollection<IActionWrapper> wrappedActions) {
            var eventRow = await Retrieve<EventRow>(eventParent.Id.ToString());
            eventRow.IsStarted = true;
            eventRow.IsDone = true;
            eventRow.IsSuccessful = true;
            eventRow.ExecutionDate = DateTimeOffset.UtcNow;
            if (wrappedActions.Any()) {
                eventRow.Actions = _jsonConverter.Serialize(wrappedActions.Select(a => a.Id));
            }
            var replaceOperation = TableOperation.Replace(eventRow);
            await _eventTable.ExecuteAsync(replaceOperation);
            if (!wrappedActions.Any()) {
                return;
            }
            var batchOperation = new TableBatchOperation();
            foreach (var wrappedAction in wrappedActions) {
                wrappedAction.JobId = eventParent.JobId;
                batchOperation.Add(TableOperation.Insert(new CommandRow {
                    Type = wrappedAction.Command.GetType().FullName,
                    RowKey = wrappedAction.Id.ToString(),
                    PartitionKey = "Command",
                    Timestamp = DateTimeOffset.UtcNow,
                    IsDone = false,
                    IsStarted = false,
                    IsSuccessful = false,
                    ExecutionDate = null,
                    ParentId = eventParent.Id
                }));
            }
            await _commandTable.ExecuteBatchAsync(batchOperation);
        }


        public async Task Done(ICommandWrapper commandWrapper) {
            var commandRow = await Retrieve<CommandRow>(commandWrapper.Id.ToString());
            commandRow.IsDone = true;
            commandRow.IsSuccessful = true;
            commandRow.IsStarted = true;
            commandRow.ExecutionDate = DateTimeOffset.UtcNow;
            var replaceOperation = TableOperation.Replace(commandRow);
            await _commandTable.ExecuteAsync(replaceOperation);
        }

        public async Task Done(IEventWrapper eventWrapper) {
            var eventRow = await Retrieve<EventRow>(eventWrapper.Id.ToString());
            eventRow.IsDone = true;
            eventRow.IsSuccessful = true;
            eventRow.IsStarted = true;
            eventRow.ExecutionDate = DateTimeOffset.UtcNow;
            var replaceOperation = TableOperation.Replace(eventRow);
            await _eventTable.ExecuteAsync(replaceOperation);
        }

        private async Task CleanupTables(IJob job) {
            foreach (var jobCommandInformation in job.CommandInformations) {
                await CleanupTables(jobCommandInformation);
            }
            var jobRow = await Retrieve<JobRow>(job.Id.ToString());
            var deleteJobOperation = TableOperation.Delete(jobRow);
            await _jobTable.ExecuteAsync(deleteJobOperation);
        }

        private async Task CleanupTables(CommandInformation commandInformation) {
            if (commandInformation.EventInformations != null && commandInformation.EventInformations.Any()) {
                foreach (var eventInformation in commandInformation.EventInformations) {
                    await CleanupTables(eventInformation);
                }
            }
            var commandRow = await Retrieve<CommandRow>(commandInformation.Id.ToString());
            var deleteCommandOperation = TableOperation.Delete(commandRow);
            await _commandTable.ExecuteAsync(deleteCommandOperation);
        }

        private async Task CleanupTables(EventInformation eventInformation) {
            if (eventInformation.CommandInformations != null && eventInformation.CommandInformations.Any()) {
                foreach (var commandInformation in eventInformation.CommandInformations) {
                    await CleanupTables(commandInformation);
                }
            }
            var eventRow = await Retrieve<EventRow>(eventInformation.Id.ToString());
            var deleteEventOperation = TableOperation.Delete(eventRow);
            await _eventTable.ExecuteAsync(deleteEventOperation);
        }

        private async Task<IReadOnlyCollection<CommandInformation>> GetCommandInformation(
            IReadOnlyCollection<Guid> identifiers) {
            var result = new List<CommandInformation>();
            foreach (var commandId in identifiers) {
                var commandRow = await Retrieve<CommandRow>(commandId.ToString());
                result.Add(await From(commandRow));
            }
            return result;
        }

        private async Task<IReadOnlyCollection<EventInformation>> GetEventInformation(
            IReadOnlyCollection<Guid> identifiers) {
            var result = new List<EventInformation>();
            foreach (var commandId in identifiers) {
                var eventRow = await Retrieve<EventRow>(commandId.ToString());
                result.Add(await From(eventRow));
            }
            return result;
        }

        private async Task<Job> From(JobRow jobRow) {
            var job = new Job {
                Id = Guid.Parse(jobRow.RowKey),
                IsDone = jobRow.IsDone
            };
            if (string.IsNullOrEmpty(jobRow.CommandIdentifiers)) {
                return job;
            }
            var commandIds = jobRow.GetCommandIdentifiers(_jsonConverter);
            if (commandIds.Any()) {
                job.CommandInformations = (await GetCommandInformation(commandIds)).ToArray();
            }
            return job;
        }

        private async Task<CommandInformation> From(CommandRow commandRow) {
            var commandInformation = new CommandInformation {
                Id = Guid.Parse(commandRow.RowKey),
                IsStarted = commandRow.IsStarted,
                IsDone = commandRow.IsDone,
                IsSuccessful = commandRow.IsSuccessful,
                ExecutionDate = commandRow.ExecutionDate,
                Type = commandRow.Type
            };
            if (string.IsNullOrEmpty(commandRow.Events)) {
                return commandInformation;
            }
            var eventIdentifiers = commandRow.GetEventIdentifiers(_jsonConverter);
            if (eventIdentifiers.Any()) {
                commandInformation.EventInformations = (await GetEventInformation(eventIdentifiers)).ToArray();
            }
            return commandInformation;
        }

        private async Task<EventInformation> From(EventRow eventRow) {
            var eventInformation = new EventInformation {
                Id = Guid.Parse(eventRow.RowKey),
                IsStarted = eventRow.IsStarted,
                IsDone = eventRow.IsDone,
                IsSuccessful = eventRow.IsSuccessful,
                ExecutionDate = eventRow.ExecutionDate,
                Type = eventRow.Type
            };
            if (string.IsNullOrEmpty(eventRow.Actions)) {
                return eventInformation;
            }
            var actionIdentifiers = eventRow.GetActionIdentifiers(_jsonConverter);
            if (actionIdentifiers.Any()) {
                eventInformation.CommandInformations = (await GetCommandInformation(actionIdentifiers)).ToArray();
            }
            return eventInformation;
        }

        protected async Task<TRow> Retrieve<TRow>(string id) where TRow : class, ITableEntity {
            var rowType = typeof(TRow);
            var retrieveOperation =
                TableOperation.Retrieve<TRow>(rowType.Name.Substring(0, rowType.Name.Length - 3), id);
            TableResult tableResult;
            switch (rowType.Name) {
                case "CommandRow":
                    tableResult = await _commandTable.ExecuteAsync(retrieveOperation);
                    break;
                case "EventRow":
                    tableResult = await _eventTable.ExecuteAsync(retrieveOperation);
                    break;
                case "JobRow":
                    tableResult = await _jobTable.ExecuteAsync(retrieveOperation);
                    break;
                default:
                    throw new ArgumentException(nameof(TRow));
            }
            var row = tableResult.Result as TRow;
            if (row == null) {
                throw new Exception();
            }
            return row;
        }

        #region Private classes

        private class JobRow : TableEntity {
            public bool IsStarted { get; set; }
            public bool IsDone { get; set; }
            public string CommandIdentifiers { get; set; }

            public IReadOnlyCollection<Guid> GetCommandIdentifiers(IJsonConverter jsonConverter) {
                return jsonConverter.Deserialize<List<Guid>>(CommandIdentifiers);
            }
        }

        private class CommandRow : TableEntity {
            public bool IsStarted { get; set; }
            public bool IsDone { get; set; }
            public DateTimeOffset? ExecutionDate { get; set; }
            public bool IsSuccessful { get; set; }
            public string Events { get; set; }
            public Guid? ParentId { get; set; }
            public string Type { get; set; }

            public IReadOnlyCollection<Guid> GetEventIdentifiers(IJsonConverter jsonConverter) {
                return jsonConverter.Deserialize<List<Guid>>(Events);
            }
        }

        private class EventRow : TableEntity {
            public bool IsStarted { get; set; }
            public bool IsDone { get; set; }
            public DateTimeOffset? ExecutionDate { get; set; }
            public bool IsSuccessful { get; set; }
            public Guid? ParentId { get; set; }
            public string Actions { get; set; }
            public string Type { get; set; }

            public IReadOnlyCollection<Guid> GetActionIdentifiers(IJsonConverter jsonConverter) {
                return jsonConverter.Deserialize<List<Guid>>(Actions);
            }
        }

        private class ActionRow : TableEntity {
        }

        #endregion
    }
}
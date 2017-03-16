using System;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.Processing.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Table;

namespace EventSourcing.Poc.Processing.Jobs {
    internal class JobArchive : FileStore<Job> {
        private CloudTable _archiveTable;

        public JobArchive(string storageConnectionString, string storageName, string archiveTableName, IJsonConverter jsonConverter)
            : base(storageConnectionString, storageName, jsonConverter) {
            _archiveTable = CloudStorageAccount.Parse(storageConnectionString)
                .CreateCloudTableClient()
                .GetTableReference(archiveTableName);
            _archiveTable.CreateIfNotExistsAsync().Wait();
        }

        protected override async Task<CloudFileDirectory> GetDestinationFolder(CloudFileShare fileShare) {
            var currentDate = DateTimeOffset.UtcNow;
            var root = fileShare.GetRootDirectoryReference();
            var yearFolder = root.GetDirectoryReference(currentDate.Year.ToString("D4"));
            await yearFolder.CreateIfNotExistsAsync();
            var monthFolder = yearFolder.GetDirectoryReference(currentDate.Month.ToString("D2"));
            await monthFolder.CreateIfNotExistsAsync();
            var dayFolder = monthFolder.GetDirectoryReference(currentDate.Day.ToString("D2"));
            await dayFolder.CreateIfNotExistsAsync();
            return dayFolder;
        }

        protected override string CreateFileName(Job @object) {
            return $"{@object.Id}.json";
        }

        public async Task Archive(Job job) {
            var archiveRow = new ArchiveRow {
                PartitionKey = "Archive",
                RowKey = job.Id.ToString(),
                Timestamp = DateTimeOffset.UtcNow,                
            };
            var path = await SaveAsync(job);
            archiveRow.Path = path;
            var insertOperation = TableOperation.Insert(archiveRow);
            await _archiveTable.ExecuteAsync(insertOperation);
        }

        public async Task<IJob> Get(Guid id) {
            var retrieveOperation = TableOperation.Retrieve<ArchiveRow>("Archive", id.ToString());
            var tableResult = await _archiveTable.ExecuteAsync(retrieveOperation);
            var archiveRow = tableResult.Result as ArchiveRow;
            if (archiveRow == null) {
                return null;
            }
            return await RetrieveAsync($"{id}.json", archiveRow.Path);
        }

        private class ArchiveRow : TableEntity {
            public string Path { get; set; }
        }
    }
}
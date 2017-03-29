using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.StorageAccount {
    public class TableClient<TEntity> : TableClient where TEntity : ITableEntity {
        public TableClient(string connectionString, string name) : base(connectionString, name) {
        }

        public TableClient(CloudTable cloudTable) : base(cloudTable) {
        }

        public Task<TableResult> Retrieve(string partitionKey, string rowKey,
            List<string> selectColumns = null) {
            return Retrieve<TEntity>(partitionKey, rowKey, selectColumns);
        }

        public Task<TableResult> Retrieve(string partitionKey,
            string rowKey,
            EntityResolver<TEntity> resolver,
            List<string> selectColumns = null) {
            return Retrieve<TEntity>(partitionKey, rowKey, resolver, selectColumns);
        }
    }

    public class TableClient {
        private readonly CloudTable _cloudTable;

        public TableClient(string connectionString, string name) {
            _cloudTable = CloudStorageAccount.Parse(connectionString)
                .CreateCloudTableClient()
                .GetTableReference(name);
        }

        public TableClient(CloudTable cloudTable) {
            _cloudTable = cloudTable;
        }

        public Task<bool> CreateIfNotExistsAsync() {
            return _cloudTable.CreateIfNotExistsAsync();
        }

        public virtual Task<TableResult> Retrieve<TEntity>(string partitionKey, string rowKey,
            List<string> selectColumns = null) where TEntity : ITableEntity {
            var retrieveOperation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey, selectColumns);
            return ExecuteAsync(retrieveOperation);
        }

        public Task<TableResult> Retrieve<TEntity>(string partitionKey,
            string rowKey,
            EntityResolver<TEntity> resolver,
            List<string> selectColumns = null) where TEntity : ITableEntity {
            var retrieveOperation = TableOperation.Retrieve(partitionKey, rowKey, resolver, selectColumns);
            return ExecuteAsync(retrieveOperation);
        }

        public Task<TableResult> Insert(ITableEntity @object) {
            var insertOperation = TableOperation.Insert(@object);
            return ExecuteAsync(insertOperation);
        }

        public Task<TableResult> ExecuteAsync(TableOperation tableOperation) {
            return _cloudTable.ExecuteAsync(tableOperation);
        }

        public Task<TableQuerySegment<T>> ExecuteQuerySegmentedAsync<T>(TableQuery<T> query, TableContinuationToken tableContinuationToken) where T : ITableEntity, new() {
            return _cloudTable.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
        }
    }
}
using System;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Mutex;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace EventSourcing.Poc.Commons.Mutex {
    public class EntityMutexFactory : IEntityMutexFactory {
        private readonly CloudTable _entityTable;

        public EntityMutexFactory(IOptions<EntityMutexFactoryOptions> options) {
            _entityTable = CloudStorageAccount.Parse(options.Value.ConnectionString)
                .CreateCloudTableClient()
                .GetTableReference(options.Value.Name);
            _entityTable.CreateIfNotExistsAsync().Wait();
        }

        public EntityMutexFactory(CloudTable entityTable) {
            _entityTable = entityTable;
            _entityTable.CreateIfNotExistsAsync().Wait();
        }

        public IMutexAsync Create<TKey>(IEntity<TKey> entityToLock) {
            return new EntityMutex<TKey>(entityToLock, _entityTable);
        }
    }

    public class EntityMutex<TKey> : IMutexAsync {
        private readonly CloudTable _entityTable;
        private readonly string _type;
        private readonly string _identifier;

        public EntityMutex(IEntity<TKey> entityToLock, CloudTable entityTable) {
            _entityTable = entityTable;
            _type = entityToLock.GetType().FullName;
            _identifier = entityToLock.Id.ToString();
        }

        public void Dispose() {
            ReleaseAsync().Wait();
        }

        public async Task LockAsync() {
            var retrieveOperation = TableOperation.Retrieve<EntityLockRow>(_type, _identifier);
            TableResult tableResult = null;
            do {
                if (tableResult != null) {
                    await Task.Delay(1000);
                }
                tableResult = await _entityTable.ExecuteAsync(retrieveOperation);
            } while (tableResult.Result != null);
            var insertOperation = TableOperation.Insert(new EntityLockRow {
                PartitionKey = _type,
                RowKey = _identifier,
                Timestamp = DateTimeOffset.UtcNow
            });
            try {
                await _entityTable.ExecuteAsync(insertOperation);
            }
            catch (StorageException e) {
                if (e.Message == "Conflict") {
                    await LockAsync();
                }
                else {
                    throw;
                }
            }
        }

        public async Task ReleaseAsync() {
            var retrieveOperation = TableOperation.Retrieve<EntityLockRow>(_type, _identifier);
            var tableResult = await _entityTable.ExecuteAsync(retrieveOperation);
            if (tableResult.Result != null) {
                var deleteOperation = TableOperation.Delete(tableResult.Result as EntityLockRow);
                await _entityTable.ExecuteAsync(deleteOperation);
            }
        }

        private class EntityLockRow : TableEntity {
            
        }
    }

    public class EntityMutexFactoryOptions {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Mutex;
using Microsoft.Azure.StorageAccount;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace EventSourcing.Poc.Processing.Mutex {
    public class EntityMutex<TKey> : IMutexAsync {
        private enum StatusType {
            Lock = 1,
            UnLock = 0
        }
        private readonly TableClient _entityTable;
        private readonly string _identifier;
        private readonly string _mutexId;
        private readonly string _type;
        private StatusType Status;

        public EntityMutex(IEntity<TKey> entityToLock, TableClient entityTable) {
            _entityTable = entityTable;
            _type = entityToLock.GetType().FullName;
            _identifier = entityToLock.Id.ToString();
            _mutexId = Guid.NewGuid().ToString();
            Status = StatusType.UnLock;
        }

        public void Dispose() {
            ReleaseAsync().Wait();
        }

        public async Task LockAsync() {
            (string partitionKey, DateTimeOffset timestamp, int magicNumber) lockData = await InitLock();

            EntityLockRow currentLocker = null;
            do {
                if (currentLocker != null) {
                    await Task.Delay(1000);
                }
                currentLocker = await GetCurrentLock(lockData);
            } while (currentLocker.RowKey != _mutexId && currentLocker.MagicNumber != lockData.magicNumber);
            Status = StatusType.Lock;
        }

        public async Task ReleaseAsync() {
            if (Status == StatusType.UnLock) {
                return;
            }
            var tableResult = await _entityTable.Retrieve<EntityLockRow>(PartitionKey, _mutexId);
            if (tableResult.Result != null) {
                var deleteOperation = TableOperation.Delete(tableResult.Result as EntityLockRow);
                await _entityTable.ExecuteAsync(deleteOperation);
                Status = StatusType.UnLock;
            }
        }

        private async Task<EntityLockRow> GetCurrentLock((string partitionKey, DateTimeOffset timestamp, int magicKey) lockData) {
            var query = new TableQuery<EntityLockRow>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, lockData.partitionKey));
            TableContinuationToken tableContinuationToken = null;
            var result = new List<EntityLockRow>();
            do {
                var queryResponse = await _entityTable.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
                tableContinuationToken = queryResponse.ContinuationToken;
                result.AddRange(queryResponse.Results);
            } while (tableContinuationToken != null);
            var filtered = result.Where(e => e.Timestamp <= lockData.timestamp).OrderBy(e => e.Timestamp).Take(1);
            return filtered.FirstOrDefault();
        }

        private int GetSeed() {
            var seed = 0;
            foreach (var @char in _mutexId) {
                if (char.IsNumber(@char)) {
                    seed -= @char;
                }
                else {
                    seed += @char;
                }
            }
            return seed;
        }

        private async Task<ValueTuple<string, DateTimeOffset, int>> InitLock() {
            var rand = new Random(GetSeed());
            var partitionKey = PartitionKey;
            var entityLockRow = new EntityLockRow {
                PartitionKey = partitionKey,
                RowKey = _mutexId,
                Timestamp = DateTimeOffset.UtcNow,
                MagicNumber = rand.Next()
            };
            await _entityTable.Insert(entityLockRow);
            return (partitionKey, entityLockRow.Timestamp, entityLockRow.MagicNumber);
        }

        private string PartitionKey => $"{_type}:{_identifier}";

        private class EntityLockRow : TableEntity {
            public int MagicNumber { get; set; }
        }
    }
}
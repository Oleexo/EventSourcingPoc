using System;
using System.Collections.Generic;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Mutex;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Azure.StorageAccount;
using Microsoft.Extensions.Options;

namespace EventSourcing.Poc.Processing.Mutex {
    public class EntityMutexFactory : IEntityMutexFactory {
        private readonly TableClient _entityTable;
        private readonly IMutexGarbageCollector _mutexGc;

        public EntityMutexFactory(IOptions<EntityMutexFactoryOptions> options, IMutexGarbageCollector mutexGc) {
            _mutexGc = mutexGc;
            _entityTable = new TableClient(options.Value.ConnectionString, options.Value.Name);
            _entityTable.CreateIfNotExistsAsync().Wait();
            _mutexGc = mutexGc;
        }

        public EntityMutexFactory(TableClient entityTable, IMutexGarbageCollector mutexGc) {
            _entityTable = entityTable;
            _entityTable.CreateIfNotExistsAsync().Wait();
            _mutexGc = mutexGc;
        }

        public IMutexAsync Create<TKey>(IEntity<TKey> entityToLock) {
            var entityMutex = new EntityMutex<TKey>(entityToLock, _entityTable);
            _mutexGc.Register(entityMutex);
            return entityMutex;
        }
    }

    public class MutexGarbageCollector : IMutexGarbageCollector {
        private readonly ICollection<IDisposable> _mutexes;

        public MutexGarbageCollector() {
            _mutexes = new List<IDisposable>();
        }

        public void Register(IMutex mutex) {
            _mutexes.Add(mutex);
        }

        public void Register(IMutexAsync mutex) {
            _mutexes.Add(mutex);
        }

        public void Collect() {
            foreach (var mutex in _mutexes) {
                mutex.Dispose();
            }
        }
    }

    public interface IMutexGarbageCollector {
        void Register(IMutex mutex);
        void Register(IMutexAsync mutex);
        void Collect();
    }
}
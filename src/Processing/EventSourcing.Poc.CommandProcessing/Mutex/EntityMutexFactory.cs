using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Mutex;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Azure.StorageAccount;
using Microsoft.Extensions.Options;

namespace EventSourcing.Poc.Processing.Mutex {
    public class EntityMutexFactory : IEntityMutexFactory {
        private readonly TableClient _entityTable;
        private readonly IMutexCollector _mutexGc;

        public EntityMutexFactory(IOptions<EntityMutexFactoryOptions> options, IMutexCollector mutexGc) {
            _mutexGc = mutexGc;
            _entityTable = new TableClient(options.Value.ConnectionString, options.Value.Name);
            _entityTable.CreateIfNotExistsAsync().Wait();
            _mutexGc = mutexGc;
        }

        public EntityMutexFactory(TableClient entityTable, IMutexCollector mutexGc) {
            _entityTable = entityTable;
            _entityTable.CreateIfNotExistsAsync().Wait();
            _mutexGc = mutexGc;
        }

        public IMutexAsync Create<TKey>(IEntity<TKey> entityToLock) {
            var entityMutex = new EntityMutex<TKey>(entityToLock, _entityTable);
            _mutexGc.Register(entityMutex);
            return entityMutex;
        }

        public async Task<IMutexAsync> CreateAndLock<TKey>(IEntity<TKey> entityToLock) {
            var mutex = Create(entityToLock);
            await mutex.LockAsync();
            return mutex;
        }
    }
}
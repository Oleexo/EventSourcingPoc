using System.Threading.Tasks;

namespace EventSourcing.Poc.EventSourcing.Mutex {
    public interface IEntityMutexFactory {
        IMutexAsync Create<TKey>(IEntity<TKey> entityToLock);
        Task<IMutexAsync> CreateAndLock<TKey>(IEntity<TKey> entityToLock);
    }
}
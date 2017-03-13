namespace EventSourcing.Poc.EventSourcing.Mutex {
    public interface IEntityMutexFactory {
        IMutexAsync Create<TKey>(IEntity<TKey> entityToLock);
    }
}
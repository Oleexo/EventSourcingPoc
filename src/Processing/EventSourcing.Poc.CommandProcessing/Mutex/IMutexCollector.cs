using EventSourcing.Poc.EventSourcing.Mutex;

namespace EventSourcing.Poc.Processing.Mutex {
    public interface IMutexCollector {
        void Register(IMutex mutex);
        void Register(IMutexAsync mutex);
        void Collect();
    }
}
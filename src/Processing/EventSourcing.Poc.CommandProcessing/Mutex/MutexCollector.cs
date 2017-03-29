using System;
using System.Collections.Generic;
using EventSourcing.Poc.EventSourcing.Mutex;

namespace EventSourcing.Poc.Processing.Mutex {
    public class MutexCollector : IMutexCollector {
        private readonly ICollection<IDisposable> _mutexes;

        public MutexCollector() {
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
}
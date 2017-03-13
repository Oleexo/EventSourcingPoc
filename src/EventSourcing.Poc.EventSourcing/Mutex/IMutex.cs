using System;
using System.Threading.Tasks;

namespace EventSourcing.Poc.EventSourcing.Mutex {
    public interface IMutex : IDisposable {
        void Lock();
        void Release();
    }

    public interface IMutexAsync : IDisposable {
        Task LockAsync();
        Task ReleaseAsync();
    }
}
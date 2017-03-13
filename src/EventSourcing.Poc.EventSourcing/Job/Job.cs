using System;

namespace EventSourcing.Poc.EventSourcing.Job {
    public class Job : IJob {
        public Guid Id { get; set; }
    }

    public interface IJob {
        Guid Id { get; set; }
    }
}
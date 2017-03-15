using System;
using System.Collections.Generic;

namespace EventSourcing.Poc.EventSourcing.Jobs {
    public class Job : IJob {
        public Job() {
            IsStarted = false;
            IsDone = false;
        }

        public Guid Id { get; set; }
        public bool IsStarted { get; set; }
        public bool IsDone { get; set; }
        public ICollection<Guid> CommandIdentifiers { get; set; }
    }

    public interface IJob {
        Guid Id { get; set; }
        bool IsStarted { get; set; }
        bool IsDone { get; set; }
        ICollection<Guid> CommandIdentifiers { get; set; }
    }

    public class CommandInformation {
        public Guid Id { get; set; }
        public bool IsStarted { get; set; }
        public bool IsDone { get; set; }
    }

    public class ActionInformation {
        public Guid Id { get; set; }
        public bool IsStarted { get; set; }
        public bool IsDone { get; set; }

    }

    public class EventInformation {
        public Guid Id { get; set; }
        public bool IsStarted { get; set; }
        public bool IsDone { get; set; }

    }
}
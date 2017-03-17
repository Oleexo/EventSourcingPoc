using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing.Poc.EventSourcing.Jobs {
    public sealed class Job : IJob {
        public Job() {
            IsDone = false;
        }

        public Guid Id { get; set; }
        public bool IsDone { get; set; }
        public ICollection<CommandInformation> CommandInformations { get; set; }

        public bool CheckAllDependenciesDone() {
            return CommandInformations.All(ci => ci.IsDone && ci.CheckAllDependenciesDone());
        }
    }

    public interface IJob {
        Guid Id { get; set; }
        bool IsDone { get; set; }
        ICollection<CommandInformation> CommandInformations { get; set; }
    }

    public class CommandInformation {
        public CommandInformation(Guid id) {
            Id = id;
        }

        public CommandInformation() {
        }

        public Guid Id { get; set; }
        public bool IsStarted { get; set; }
        public bool IsDone { get; set; }
        public bool IsSuccessful { get; set; }
        public ICollection<EventInformation> EventInformations { get; set; }
        public DateTimeOffset? ExecutionDate { get; set; }
        public string Type { get; set; }

        public bool CheckAllDependenciesDone() {
            if (EventInformations == null || !EventInformations.Any()) {
                return true;
            }

            return EventInformations.All(ei => ei.IsDone && ei.CheckAllDependenciesDone());
        }
    }

    public class EventInformation {
        public Guid Id { get; set; }
        public bool IsStarted { get; set; }
        public bool IsDone { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTimeOffset? ExecutionDate { get; set; }
        public IReadOnlyCollection<CommandInformation> CommandInformations { get; set; }
        public string Type { get; set; }

        public bool CheckAllDependenciesDone() {
            if (CommandInformations == null || !CommandInformations.Any()) {
                return true;
            }
            return CommandInformations.All(ci => ci.IsDone && ci.CheckAllDependenciesDone());
        }
    }
}
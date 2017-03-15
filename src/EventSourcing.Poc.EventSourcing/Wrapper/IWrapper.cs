using System;

namespace EventSourcing.Poc.EventSourcing.Wrapper {
    public interface IWrapper {
        Guid Id { get; set; }
        bool IsLinkToJob { get; }
    }
}
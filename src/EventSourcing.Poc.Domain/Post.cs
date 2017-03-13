using System;
using EventSourcing.Poc.EventSourcing;

namespace EventSourcing.Poc.Domain {
    public class Post : IEntity<Guid> {
        public Guid Id { get; set; }
    }
}
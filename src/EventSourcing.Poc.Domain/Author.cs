using System;
using EventSourcing.Poc.EventSourcing;

namespace EventSourcing.Poc.Domain {
    public class Author : IEntity<Guid> {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public Guid Id { get; set; }
    }
}
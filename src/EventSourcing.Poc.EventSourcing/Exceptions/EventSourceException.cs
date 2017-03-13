using System;

namespace EventSourcing.Poc.EventSourcing.Exceptions {
    public class EventSourceException : Exception {
        public EventSourceException() {
        }

        public EventSourceException(string message) : base(message) {
        }

        public EventSourceException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
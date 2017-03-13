using System;

namespace EventSourcing.Poc.EventSourcing.Exceptions {
    public abstract class DomainException : Exception {
        public DomainException() {
        }

        public DomainException(string message) : base(message) {
        }

        public DomainException(string message, Exception innerException) : base(message, innerException) {
        }

        public abstract ExceptionDetail GetDetail();
    }
}
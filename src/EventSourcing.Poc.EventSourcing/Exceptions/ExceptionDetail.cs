using System;

namespace EventSourcing.Poc.EventSourcing.Exceptions {
    public class ExceptionDetail {
        public string ExceptionType { get; set; }
        public string ExceptionSerialized { get; set; }

        public static ExceptionDetail Create(Exception exception) {
            if (exception is DomainException) {
                return ((DomainException) exception).GetDetail();
            }
            return new ExceptionDetail {
                ExceptionType = exception.GetType().FullName,
                ExceptionSerialized = string.Empty //exception.ToJson()
            };
        }
    }
}
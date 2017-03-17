using System;

namespace EventSourcing.Poc.EventSourcing.Utils {
    public interface IJsonConverter {
        string Serialize(object @object);
        T Deserialize<T>(string json);
        object Deserialize(string json, Type outputType);
    }
}
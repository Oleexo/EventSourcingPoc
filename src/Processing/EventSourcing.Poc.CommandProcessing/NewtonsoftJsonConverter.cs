using System;
using EventSourcing.Poc.EventSourcing.Utils;
using Newtonsoft.Json;

namespace EventSourcing.Poc.Processing {
    public class NewtonsoftJsonConverter : IJsonConverter {
        public string Serialize(object @object) {
            return JsonConvert.SerializeObject(@object);
        }

        public T Deserialize<T>(string json) {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public object Deserialize(string json, Type outputType) {
            return JsonConvert.DeserializeObject(json, outputType);
        }
    }
}
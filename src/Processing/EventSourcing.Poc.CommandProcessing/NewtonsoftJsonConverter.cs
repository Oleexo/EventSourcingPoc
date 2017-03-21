using System;
using EventSourcing.Poc.EventSourcing.Utils;
using Newtonsoft.Json;

namespace EventSourcing.Poc.Processing {
    public class NewtonsoftJsonConverter : IJsonConverter {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public NewtonsoftJsonConverter() {
            _jsonSerializerSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
        }

        public string Serialize(object @object) {
            return JsonConvert.SerializeObject(@object, _jsonSerializerSettings);
        }

        public T Deserialize<T>(string json) {
            return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
        }

        public object Deserialize(string json, Type outputType) {
            return JsonConvert.DeserializeObject(json, outputType, _jsonSerializerSettings);
        }
    }
}
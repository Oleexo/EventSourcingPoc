using System;
using EventSourcing.Poc.EventSourcing.Utils;

namespace EventSourcing.Poc.Processing
{
    public class NewtonsoftJsonConverter : IJsonConverter
    {
        public string Serialize(object @object) {
            return Newtonsoft.Json.JsonConvert.SerializeObject(@object);
        }

        public T Deserialize<T>(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public object Deserialize(string json, Type outputType) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, outputType);
        }
    }
}

using System;
using System.Text;

namespace EventSourcing.Poc.EventSourcing.Utils {
    public static class SerializationExtentions {
        public static string ToBase64(this string @string) {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(@string));
        }

        public static string FromBase64(this string encodedString) {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
        }

        //public static string ToJsonBase64(this object @object)
        //{
        //    return Convert.ToBase64String(Encoding.UTF8.GetBytes(@object.ToJson()));
        //}

        //public static T FromJsonBase64<T>(this string encodedValue)
        //{
        //    return Encoding.UTF8.GetString(Convert.FromBase64String(encodedValue))
        //        .FromJson<T>();
        //}

        //public static object FromJsonBase64(this string encodedValue, Type type) {
        //    return Encoding.UTF8.GetString(Convert.FromBase64String(encodedValue))
        //        .FromJson(type);
        //}

        //public static string ToJson(this object @object)
        //{
        //    return JsonConvert.SerializeObject(@object, new JsonSerializerSettings {
        //        Formatting = Formatting.Indented,
        //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //    });
        //}

        //public static T FromJson<T>(this string jsonValue)
        //{
        //    return JsonConvert.DeserializeObject<T>(jsonValue);
        //}

        //public static object FromJson(this string jsonValue, Type type) {
        //    return JsonConvert.DeserializeObject(jsonValue, type);
        //}
    }
}
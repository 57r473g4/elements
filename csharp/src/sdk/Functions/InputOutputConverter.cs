using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Hypar.Functions
{
    /// <summary>
    /// Converter for types which inherit from ParameterData
    /// </summary>
    public class InputOutputConverter : JsonConverter
    {
        /// <summary>
        /// Can this converter convert an object of the specified type?
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(InputOutputBase);
        }

        /// <summary>
        /// Can this converter write json?
        /// </summary>
        public override bool CanWrite
        {
            get{return false;}
        }

        /// <summary>
        /// Read json.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            string typeName = (jsonObject["type"]).ToString();
            switch(typeName)
            {
                case "number":
                    return jsonObject.ToObject<NumberParameter>(serializer);
                case "range":
                    return jsonObject.ToObject<RangeParameter>(serializer);
                case "point":
                    return jsonObject.ToObject<PointParameter>(serializer);
                case "location":
                    return jsonObject.ToObject<LocationParameter>(serializer);
                case "data":
                    return jsonObject.ToObject<DataParameter>(serializer);
                default:
                    return jsonObject.ToObject<InputOutputBase>(serializer);
            }
        }

        /// <summary>
        /// Write json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
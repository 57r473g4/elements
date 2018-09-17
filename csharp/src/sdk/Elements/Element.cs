using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;
using Newtonsoft.Json.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public abstract class Element
    {
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();

        /// <summary>
        /// The unique identifier of the Element.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("id")]
        public string Id {get;internal set;}

        /// <summary>
        /// The type of the element.
        /// </summary>
        [JsonProperty("type")]
        public virtual string Type
        {
            get{ return "element";}
        }

        /// <summary>
        /// A map of Parameters for the Element.
        /// </summary>
        /// <value></value>
        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters
        {
            get{return _parameters;}
        }

        /// <summary>
        /// The element's material.
        /// </summary>
        [JsonProperty("material")]
        public Material Material{get; protected set;}
        
        /// <summary>
        /// The element's transform.
        /// </summary>
        [JsonProperty("transform")]
        public Transform Transform{get; protected set;}

        /// <summary>
        /// Construct a default Element.
        /// </summary>
        public Element()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Add a Parameter to the Element.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameter">The parameter to add.</param>
        /// <returns></returns>
        public void AddParameter<T>(string name, Parameter<T> parameter)
        {
            if(!_parameters.ContainsKey(name))
            {
                _parameters.Add(name, parameter);
                return;
            }

            throw new Exception($"The parameter, {name}, already exists");
        }

        /// <summary>
        /// Remove a Parameter from the Parameters map.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveParameter(string name)
        {
            if(_parameters.ContainsKey(name))
            {
                _parameters.Remove(name);
            } 
            
            throw new Exception("The specified parameter could not be found.");
        }
    }

    public class ElementConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Element);
        }

        public override bool CanRead
        {
            get{return true;}
        }

        public override bool CanWrite
        {
            get{return false;}
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeName = (string)obj.GetValue("type");
            switch(typeName)
            {
                case "panel":
                    return obj.ToObject<Panel>();
                case "floor":
                    return obj.ToObject<Floor>();
                case "mass":
                    return obj.ToObject<Mass>();
                case "space":
                    return obj.ToObject<Space>();
                case "column":
                    return obj.ToObject<Column>();
                case "beam":
                    return obj.ToObject<Beam>();
                case "brace":
                    return obj.ToObject<Brace>();
                default:
                    throw new Exception($"The object with type name, {typeName}, could not be deserialzed.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TDPG.Generators.Seed;

namespace TDPG.EffectSystem.ElementLogic
{
    public class ElementConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Element).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var element = (Element)value;

            var jo = new JObject
            {
                ["Name"] = element.Name,
                ["Id"] = element.Id,
                ["MetaData"] = JArray.FromObject(element.MetaData, serializer),
                ["Dna"] = JToken.FromObject(element.GetDna(), serializer),
                ["Effects"] = JArray.FromObject(element.GetEffects(), serializer)
            };

            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            string name = (string)jo["Name"];
            int id = (int)jo["Id"];
            var dna = jo["Dna"].ToObject<Seed>(serializer);
            var effects = jo["Effects"].ToObject<List<Effect>>(serializer);
            var meta = jo["MetaData"].ToObject<List<string>>(serializer) ?? new List<string>();

            // Construct the element using the constructor that takes effects (will auto-set MetaData to DNA string)
            var element = new Element(name, id, dna, effects);

            // Replace MetaData entirely instead of merging
            element.MetaData = meta;

            return element;
        }

        
        
    }
}
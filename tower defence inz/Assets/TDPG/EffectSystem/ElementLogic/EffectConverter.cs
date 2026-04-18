using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TDPG.EffectSystem.ElementLogic
{
    public class EffectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Effect effect = (Effect)value;
            
            // Create an envelope with type info + actual data
            var jo = new JObject
            {
                ["Type"] = effect.GetType().FullName,
                ["Assembly"] = effect.GetType().Assembly.GetName().Name,
                ["Name"] = effect.Name,
                ["Description"] = effect.Description,
                ["Values"] = JArray.FromObject(effect.GetValues(), serializer)
            };

            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            string typeName = (string)jo["Type"];
            string assemblyName = (string)jo["Assembly"];

            Type type = Type.GetType($"{typeName}, {assemblyName}"); //Uses .NET’s reflection to resolve the Type by its fully qualified name + assembly name.
            if (type == null)
                throw new JsonSerializationException($"Unknown effect type: {typeName}");

            float[] values = jo["Values"].ToObject<float[]>(serializer); //deserialize values to new float[]

            // Try to find a constructor with exactly N float parameters {Asks reflection: “Give me all public constructors of this type.”}
            var ctor = type.GetConstructors()
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == values.Length && p.All(x => x.ParameterType == typeof(float));
                });

            if (ctor == null)
                throw new JsonSerializationException($"No matching constructor found for {typeName} with {values.Length} floats");

            // Create instance
            var instance = (Effect)ctor.Invoke(values.Cast<object>().ToArray());
            return instance;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Effect).IsAssignableFrom(objectType);
        }
    }
}
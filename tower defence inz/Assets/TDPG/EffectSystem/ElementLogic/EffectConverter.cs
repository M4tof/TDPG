using System;
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

            Type type = Type.GetType($"{typeName}, {assemblyName}");
            if (type == null)
                throw new JsonSerializationException($"Unknown effect type: {typeName}");

            // Read values
            float[] values = jo["Values"].ToObject<float[]>(serializer);

            // Reconstruct instance
            Effect instance;
            if (type == typeof(SlowDown))
                instance = new SlowDown(values[0], values[1]);
            else if (type == typeof(HealthDown))
                instance = new HealthDown(values[0]);
            else if (type == typeof(Heal))
                instance = new Heal(values[0]);
            else
                throw new JsonSerializationException($"Unsupported effect type: {typeName}");
            //TODO: expand with each new EffecType

            return instance;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Effect).IsAssignableFrom(objectType);
        }
    }
}
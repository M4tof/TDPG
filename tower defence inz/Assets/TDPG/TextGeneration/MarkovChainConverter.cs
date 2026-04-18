using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TDPG.TextGeneration
{
    public class MarkovChainConverter : JsonConverter
    {
        private void WriteField(JObject jo, string fieldName, object target, Type t)
        {
            var field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                var val = field.GetValue(target);
                jo[fieldName] = val != null ? JToken.FromObject(val) : null;
            }
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            MarkovChain mc = (MarkovChain)value;
            Type t = mc.GetType();

            var jo = new JObject
            {
                ["Type"] = t.FullName,
                ["Assembly"] = t.Assembly.GetName().Name
            };

            // Extract all private / serialized fields via reflection
            WriteField(jo, "order", mc, t);
            WriteField(jo, "chain", mc, t);
            WriteField(jo, "forcedPrefixes", mc, t);
            WriteField(jo, "forcedSuffixes", mc, t);
            WriteField(jo, "prefixWeights", mc, t);
            WriteField(jo, "suffixWeights", mc, t);
            WriteField(jo, "blacklist", mc, t);

            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            string typeName = (string)jo["Type"];
            string assemblyName = (string)jo["Assembly"];

            Type t = Type.GetType($"{typeName}, {assemblyName}");
            if (t == null)
                throw new JsonSerializationException($"Cannot find type {typeName}");

            // ---- Reconstruct using constructor: MarkovChain(int order)
            int order = jo["order"]?.ToObject<int>() ?? 1;

            object instance = Activator.CreateInstance(t, order);

            // ---- Populate private fields
            ReadField(jo, "chain", instance, t, serializer);
            ReadField(jo, "forcedPrefixes", instance, t, serializer);
            ReadField(jo, "forcedSuffixes", instance, t, serializer);
            ReadField(jo, "prefixWeights", instance, t, serializer);
            ReadField(jo, "suffixWeights", instance, t, serializer);
            ReadField(jo, "blacklist", instance, t, serializer);

            return instance;
        }
        
        private void ReadField(JObject jo, string fieldName, object target, Type t, JsonSerializer serializer)
        {
            JToken token = jo[fieldName];
            if (token == null) return;

            var field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return;

            object value = token.ToObject(field.FieldType, serializer);
            field.SetValue(target, value);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(MarkovChain).IsAssignableFrom(objectType);
        }
        
        public override bool CanRead => true;
        public override bool CanWrite => true;
    }
}
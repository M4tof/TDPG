using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TDPG.Generators.Seed
{
    public class SeedConverter : JsonConverter<Seed>
    {
        public override void WriteJson(JsonWriter writer, Seed value, JsonSerializer serializer)
        {
            var jo = new JObject
            {
                ["Value"] = value.Value,
                ["Id"] = value.Id,
                ["ParentName"] = value.ParentName
            };
            jo.WriteTo(writer);
        }

        public override Seed ReadJson(JsonReader reader, Type objectType, Seed existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            ulong val = jo["Value"]?.ToObject<ulong>() ?? 0;
            int id = jo["Id"]?.ToObject<int>() ?? 0;
            string parent = jo["ParentName"]?.ToObject<string>();
            return new Seed(val, id, parent);
        }
    }
}
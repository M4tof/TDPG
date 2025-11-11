using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuikGraph;
using TDPG.EffectSystem.ElementLogic;
using TDPG.Generators.Seed;

namespace TDPG.EffectSystem.ElementRegistry
{
public class RegistryConverter : JsonConverter<Registry>
    {
        public override void WriteJson(JsonWriter writer, Registry value, JsonSerializer serializer)
        {
            JObject jo = new JObject
            {
                ["RootElement"] = JToken.FromObject(value.RootElement, serializer),
                ["MutateType"] = value.GetMutateType().ToString()
            };

            // Serialize all vertices (elements)
            List<Element> elements = value.GetAllElements().ToList();
            jo["Elements"] = JArray.FromObject(elements, serializer);

            // Serialize all edges
            List<JObject> edges = value.GetEdges()
                .Select(e => new JObject
                {
                    ["SourceId"] = e.Source.Id,
                    ["TargetId"] = e.Target.Id
                })
                .ToList();

            jo["Edges"] = new JArray(edges);
            jo.WriteTo(writer);
        }

        public override Registry ReadJson(JsonReader reader, Type objectType, Registry existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var mutateTypeStr = jo["MutateType"]?.ToObject<string>();
            MutateTypes mutateType = Enum.TryParse(mutateTypeStr, out MutateTypes mt) ? mt : MutateTypes.Deterministic;

            Registry registry = new Registry();
            registry.SetMutateSeedRule(mutateType);

            // Deserialize all elements
            var elements = jo["Elements"]?.ToObject<List<Element>>(serializer) ?? new List<Element>();
            foreach (var el in elements)
            {
                if (el.Id == 0 && el.Name == "Root")
                {
                    registry.OverrideRootElement(el); // replace default root from constructor
                }
                else
                {
                    // Add other elements
                    registry.AddElementWithoutEdges(el);
                }
            }

            // Deserialize edges
            var edges = jo["Edges"]?.ToObject<List<Registry.RegistryEdge>>(serializer) ?? new List<Registry.RegistryEdge>();

            foreach (var edge in edges)
            {
                Element source = registry.GetElement(edge.SourceId);
                Element target = registry.GetElement(edge.TargetId);

                if (source != null && target != null)
                {
                    // Avoid duplicate edges
                    if (!registry.registryGraph.ContainsEdge(source, target))
                        registry.registryGraph.AddEdge(new Edge<Element>(source, target));
                }
            }

            return registry;
        }
    }
}
using System;
using System.Collections.Generic;
using QuikGraph;
using TDPG.EffectSystem.ElementLogic;
using Newtonsoft.Json;
using TDPG.Generators.Seed;

namespace TDPG.EffectSystem.ElementRegistry
{
    public partial class Registry
    {
        // Helpers for converter (internal use)
        internal void OverrideRootElement(Element root)
        {
            if (rootElement != null && registryGraph.ContainsVertex(rootElement))
            {
                registryGraph.RemoveVertex(rootElement); // Remove the old root
            }

            rootElement = root;

            // Ensure the new root is in the graph
            if (!registryGraph.ContainsVertex(rootElement))
                registryGraph.AddVertex(rootElement);
        }
        
        internal void AddElementWithoutEdges(Element e)
        {
            if (!registryGraph.ContainsVertex(e))
                registryGraph.AddVertex(e);
        }

        internal void AddEdge(Element source, Element target)
        {
            if (!registryGraph.ContainsEdge(source, target))
                registryGraph.AddEdge(new Edge<Element>(source, target));
        }
        
        [Serializable]
        public class RegistryEdge
        {
            public int SourceId { get; set; }
            public int TargetId { get; set; }

            public RegistryEdge() { }

            public RegistryEdge(int sourceId, int targetId)
            {
                SourceId = sourceId;
                TargetId = targetId;
            }
        }
        
        
        private static JsonSerializerSettings DefaultSettings => new()
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new EffectConverter(),
                new ElementConverter(),
                new SeedConverter(),
                new RegistryConverter()
            }
        };

        public string Serialize() => JsonConvert.SerializeObject(this, DefaultSettings);
        public static Registry Deserialize(string json) => JsonConvert.DeserializeObject<Registry>(json, DefaultSettings);
    }
}
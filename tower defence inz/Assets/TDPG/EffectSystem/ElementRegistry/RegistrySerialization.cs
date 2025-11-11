using System;
using QuikGraph;
using TDPG.EffectSystem.ElementLogic;

namespace TDPG.EffectSystem.ElementRegistry
{
    public partial class Registry
    {
        // Helpers for converter (internal use)
        internal void OverrideRootElement(Element root)
        {
            if (rootElement != null && registryGraph.ContainsVertex(rootElement))
            {
                registryGraph.RemoveVertex(rootElement); // remove the old root
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
    }
}
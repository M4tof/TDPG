using QuikGraph;
using TDPG.EffectSystem.ElementLogic;

namespace TDPG.EffectSystem.ElementRegistry
{
    public class Registry
    {
        // The effect graph storing relationships between effect elements
        private AdjacencyGraph<Element, Edge<Element>> graph;
        private bool shouldMutate = false;
    
        public Registry()
        {
            // Directed graph (true) allows edges to have a direction (Parent -> Child)
            graph = new AdjacencyGraph<Element, Edge<Element>>(true);
        }
        
        
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using TDPG.EffectSystem.ElementLogic;
using TDPG.Generators.Seed;
using static TDPG.Generators.Seed.Genetic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementRegistry
{
    /// <summary>
    /// The central repository responsible for storing and organizing <see cref="Element"/>s.
    /// <br/>
    /// It maintains the "Evolutionary Tree" of elements using a <see cref="BidirectionalGraph{TVertex,TEdge}"/>.
    /// This allows for tracking lineage (Parent -> Child) as well as backtracking (Child -> Parent).
    /// </summary>
    [Serializable]
    public partial class Registry
    {
        private Element rootElement;
        /// <summary>
        /// The anchor point of the registry (ID = 0). 
        /// <br/>All other elements in the system eventually descend from this node.
        /// Not an element in game logic but a static entry point
        /// </summary>
        public Element RootElement => rootElement;
        
        /// <summary>
        /// The underlying graph data structure.
        /// <br/>
        /// <b>Vertices:</b> The <see cref="Element"/> instances.
        /// <br/>
        /// <b>Edges:</b> The evolutionary relationships (A created B).
        /// </summary>
        [SerializeField] internal BidirectionalGraph<Element, Edge<Element>> registryGraph;
        
        /// <summary>
        /// Defines the default mutation strategy when generating new child elements.
        /// </summary>
        [SerializeField]
        private MutateTypes mutateType = MutateTypes.Deterministic;
    
        //Constructor
        
        /// <summary>
        /// Initializes a new, empty Registry with a single "Root" element.
        /// </summary>
        public Registry()
        {
            // Directed Graph (true) allows edges to have a direction (Parent -> Child)
            registryGraph = new BidirectionalGraph<Element, Edge<Element>>(true);

            // Create a fake root element (ID = 0)
            rootElement = new Element("Root", 0, new Seed(0, -1, "Root"));

            // Add root element as the base of the registry
            registryGraph.AddVertex(rootElement);
        }

        //Params modify
        
        /// <summary>
        /// Updates the mutation strategy used for future element generation.
        /// </summary>
        /// <param name="newValue">The new mutation rule.</param>
        /// <returns>The confirmed new value.</returns>
        public MutateTypes SetMutateSeedRule(MutateTypes newValue)
        {
            this.mutateType =  newValue;
            return this.mutateType;
        }
        
        /// <summary>
        /// Retrieves all connections (Parent-Child links) currently in the graph.
        /// </summary>
        public IEnumerable<Edge<Element>> GetEdges() => registryGraph.Edges;
        
        /// <summary>
        /// Retrieves all unique Elements currently stored in the graph.
        /// </summary>
        public IEnumerable<Element> GetAllElements() => registryGraph.Vertices;
        
        /// <summary>
        /// Retrieves the current mutation configuration.
        /// </summary>
        public MutateTypes GetMutateType() => mutateType;
    }
}
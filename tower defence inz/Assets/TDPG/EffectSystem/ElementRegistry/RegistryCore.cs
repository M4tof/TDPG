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
    [Serializable]
    public partial class Registry
    {
        private Element rootElement;
        public Element RootElement => rootElement;
        
        // The effect registryGraph storing relationships between effect elements
        [SerializeField] internal BidirectionalGraph<Element, Edge<Element>> registryGraph;
        
        [SerializeField]
        private MutateTypes mutateType = MutateTypes.Deterministic;
    
        //Constructor
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
        public MutateTypes SetMutateSeedRule(MutateTypes newValue)
        {
            this.mutateType =  newValue;
            return this.mutateType;
        }
        
        public IEnumerable<Edge<Element>> GetEdges() => registryGraph.Edges;
        public IEnumerable<Element> GetAllElements() => registryGraph.Vertices;
        public MutateTypes GetMutateType() => mutateType;
    }
}
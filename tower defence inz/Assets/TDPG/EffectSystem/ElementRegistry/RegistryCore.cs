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
        [SerializeField]
        private BidirectionalGraph<Element, Edge<Element>> registryGraph;
        
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
        
        //Add elements to registry
        public bool PutPreMadeElement(List<int> parentsId, Element newElement)
        {
            if (newElement == null)
            {
                Debug.LogError("Cannot add null element to registry.");
                return false;
            }

            // Find parent elements by ID
            List<Element> parentElements = registryGraph.Vertices.Where(v => parentsId.Contains(v.Id)).ToList();
            if (parentElements.Count == 0)
            {
                Debug.LogWarning($"No parent elements found for IDs [{string.Join(",", parentsId)}].");
                return false;
            }

            newElement.Id = CountElements(); //ensure ID is next in line

            // Add new element if it's not already in the graph
            if (!registryGraph.ContainsVertex(newElement))
                registryGraph.AddVertex(newElement);

            // Create edges (parent -> child)
            foreach (Element parent in parentElements)
            {
                if (!registryGraph.ContainsEdge(parent, newElement))
                    registryGraph.AddEdge(new Edge<Element>(parent, newElement));
            }

            Debug.Log($"Added element '{newElement.Name}' (ID {newElement.Id}) under parents [{string.Join(", ", parentElements.Select(p => p.Name))}]");
            return true;
        }
        public Element GenerateChildElementFromParents(List<int> parentsId)
        {
            // Find parent elements
            List<Element> parentElements = registryGraph.Vertices.Where(v => parentsId.Contains(v.Id)).ToList();
            if (parentElements.Count == 0)
            {
                Debug.LogWarning($"No parent elements found for IDs [{string.Join(",", parentsId)}].");
                return null;
            }

            // Combine DNA and Gather all effects from parents
            List<Effect> effects = new List<Effect>();
            Seed newSeed = new Seed(0, -1, "GeneratedFromParents");
            foreach (Element parent in parentElements)
            {
                newSeed += parent.GetDna();
                List<Effect> parentEffects = parent.GetEffects();
                if (parentEffects is { Count: > 0 })
                    effects.AddRange(parentEffects);
            }
            
            newSeed = MutateSeed(newSeed, mutateType);
            
            // Select which effects are passed down based on seed bits
            newSeed.NormalizeSeedValue();
            ulong baseValue = newSeed.GetBaseValue();
            List<Effect> inheritedEffects = new List<Effect>();
            for (int i = 0; i < effects.Count; i++)
            {
                if (newSeed.IsBitSet(i))
                    inheritedEffects.Add(effects[i]);
            }
            
            // If no effects selected (e.g., all bits 0), pick at least one fallback
            if (inheritedEffects.Count == 0 && effects.Count > 0)
            {
                inheritedEffects.Add(effects[0]);
                Debug.LogWarning($"Seed {baseValue} produced no active bits — defaulted to first effect.");
            }
            
            // Combine duplicate effects intelligently based on last 2 bits of seed
            int mode = (int)(baseValue & 0b11);
            List<Effect> finalEffects = new List<Effect>();

            var grouped = inheritedEffects.GroupBy(e => e.Name);
            foreach (var group in grouped)
            {
                var effectList = group.ToList();
                Effect chosenEffect = null;

                switch (mode)
                {
                    case 0:
                        chosenEffect = effectList.First();
                        break;
                    case 1:
                        chosenEffect = effectList.Last();
                        break;
                    case 2:
                    case 3:
                        chosenEffect = AverageEffects(effectList);
                        break;
                }

                if (chosenEffect != null)
                    finalEffects.Add(chosenEffect);
            }
            
            ApplySeedBoosts(finalEffects, baseValue);
            
            // Create a unique ID for the new element
            int currId = CountElements();
            
            // Create new element  
            Element newElement = new Element($"CustomElement:{currId}", currId, newSeed, finalEffects);
            
            // Add to registry and link with parents
            if (!registryGraph.ContainsVertex(newElement))
                registryGraph.AddVertex(newElement);
            
            foreach (Element parent in parentElements)
            {
                // Parent -> Child
                if (!registryGraph.ContainsEdge(parent, newElement))
                    registryGraph.AddEdge(new Edge<Element>(parent, newElement));
            }

            
            Debug.Log($"Generated new element '{newElement.Name}' (ID {currId}) from parents [{string.Join(", ", parentElements.Select(p => p.Name))}]");
            return newElement;
        }
        
    }
}
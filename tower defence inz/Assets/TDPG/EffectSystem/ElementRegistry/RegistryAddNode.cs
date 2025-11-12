using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using TDPG.EffectSystem.ElementLogic;
using TDPG.Generators.Seed;
using UnityEngine;
using static TDPG.Generators.Seed.Genetic;

namespace TDPG.EffectSystem.ElementRegistry
{
    public partial class Registry
    { 
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

            if (newElement.Id == 0)
                newElement.Id = CountElements(); // only auto-assign if ID not set (RECOMMENDED NOT TO SET MANUALLY)

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
        
        public Element GenerateChildElementFromParents_Recombine(List<int> parentsId)
        {
            return GenerateChildElementFromParentsCore(parentsId, 
                combineSeeds: (seed, parentSeed) => seed *= parentSeed,
                fallbackEffectSelector: effects => effects.First(),
                finalEffectSelectorMode: (mode, effects) =>
                {
                    return mode switch
                    {
                        0 => effects.First(),
                        1 => effects.Last(),
                        2 or 3 => AverageEffects(effects),
                        _ => effects.First()
                    };
                },
                operationLabel: "crossing over"
            );
        }

        public Element GenerateChildElementFromParents_Addition(List<int> parentsId)
        {
            return GenerateChildElementFromParentsCore(parentsId, 
                combineSeeds: (seed, parentSeed) => seed += parentSeed,
                fallbackEffectSelector: effects => effects.Last(),
                finalEffectSelectorMode: (mode, effects) =>
                {
                    return mode switch
                    {
                        0 => AverageEffects(effects),
                        1 => effects.Last(),
                        2 or 3 => effects.First(),
                        _ => effects.First()
                    };
                },
                operationLabel: "adding"
            );
        }
        
        
        private Element GenerateChildElementFromParentsCore(
        List<int> parentsId,
        Action<Seed, Seed> combineSeeds,
        Func<List<Effect>, Effect> fallbackEffectSelector,
        Func<int, List<Effect>, Effect> finalEffectSelectorMode,
        string operationLabel)
    {
        // Find parent elements
        List<Element> parentElements = registryGraph.Vertices.Where(v => parentsId.Contains(v.Id)).ToList();
        if (parentElements.Count == 0)
        {
            Debug.LogWarning($"No parent elements found for IDs [{string.Join(",", parentsId)}].");
            return null;
        }

        // Combine DNA and gather all effects
        List<Effect> effects = new List<Effect>();
        Seed newSeed = new Seed(0, -1, "GeneratedFromParents");
        foreach (Element parent in parentElements)
        {
            combineSeeds(newSeed, parent.GetDna());
            List<Effect> parentEffects = parent.GetEffects();
            if (parentEffects is { Count: > 0 })
                effects.AddRange(parentEffects);
        }

        newSeed = MutateSeed(newSeed, mutateType);
        newSeed.NormalizeSeedValue();
        ulong baseValue = newSeed.GetBaseValue();

        // Select inherited effects based on seed bits
        List<Effect> inheritedEffects = new List<Effect>();
        for (int i = 0; i < effects.Count; i++)
        {
            if (newSeed.IsBitSet(i))
                inheritedEffects.Add(effects[i]);
        }

        // Fallback
        if (inheritedEffects.Count == 0 && effects.Count > 0)
        {
            inheritedEffects.Add(fallbackEffectSelector(effects));
            Debug.LogWarning($"Seed {baseValue} produced no active bits — defaulted to fallback effect.");
        }

        // Handle duplicates intelligently
        int mode = (int)(baseValue & 0b11);
        List<Effect> finalEffects = new List<Effect>();
        var grouped = inheritedEffects.GroupBy(e => e.Name);
        foreach (var group in grouped)
        {
            var effectList = group.ToList();
            Effect chosenEffect = finalEffectSelectorMode(mode, effectList);
            if (chosenEffect != null)
                finalEffects.Add(chosenEffect);
        }

        ApplySeedBoosts(finalEffects, baseValue);

        // Create new element
        int currId = CountElements();
        Element newElement = new Element($"CustomElement:{currId}", currId, newSeed, finalEffects); //TODO: once refactor done use one constructor

        // Add to registry and link parents
        if (!registryGraph.ContainsVertex(newElement))
            registryGraph.AddVertex(newElement);

        foreach (Element parent in parentElements)
        {
            if (!registryGraph.ContainsEdge(parent, newElement))
                registryGraph.AddEdge(new Edge<Element>(parent, newElement));
        }

        Debug.Log($"Generated new element '{newElement.Name}' (ID {currId}) from {operationLabel} parents [{string.Join(", ", parentElements.Select(p => p.Name))}]");
        return newElement;
    }

        
        
        
    }
}
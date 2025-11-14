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
            
            // Lookup all parents
            List<Element> parentElements = registryGraph.Vertices
                .Where(v => parentsId.Contains(v.Id))
                .ToList();

            if (parentElements.Count == 0)
            {
                Debug.LogWarning(
                    $"No parent elements found for IDs [{string.Join(",", parentsId)}].");
                return false;
            }

            var requestedParentSet = parentElements.Select(p => p.Id).ToHashSet();
            
            // Detect element already exists
            Element existingElement = registryGraph.Vertices
                .FirstOrDefault(v => v.Id == newElement.Id);

            if (existingElement != null)
            {
                // Pull the existing parents from the graph
                var existingParents = registryGraph.InEdges(existingElement)
                    .Select(e => e.Source.Id)
                    .ToHashSet();

                bool existingIsRootOnly =
                    existingParents.Count == 1 && existingParents.Contains(0);

                bool requestedIsRootOnly =
                    requestedParentSet.Count == 1 && requestedParentSet.Contains(0);
                
                // Reassignment rules
                if (!existingIsRootOnly || !requestedIsRootOnly)
                {
                    Debug.LogWarning(
                        $"Cannot reassign element '{existingElement.Name}' (ID {existingElement.Id}). " +
                        $"Existing parents=[{string.Join(", ", existingParents)}], " +
                        $"Requested=[{string.Join(", ", requestedParentSet)}]");
                    return false;
                }
                newElement = existingElement; 
            }
            
            // Enforce unique parent-set constraint (except root)
            if (HasElementWithSameNonRootParents(requestedParentSet))
            {
                Debug.LogWarning(
                    $"Cannot add element '{newElement.Name}' because another element already has parents " +
                    $"[{string.Join(", ", requestedParentSet)}], and only {{Root}} is allowed to have duplicate children.");
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
            // 1. Find parent elements
            List<Element> parentElements = registryGraph.Vertices
                .Where(v => parentsId.Contains(v.Id))
                .ToList();
            if (parentElements.Count == 0)
            {
                Debug.LogWarning($"No parent elements found for IDs [{string.Join(",", parentsId)}].");
                return null;
            }
            List<Effect> allParentEffects = parentElements.SelectMany(p => p.GetEffects()).ToList();
            
            // 1.5 Ensure we aren't replacing an already existing element
            var parentIdSet = parentElements.Select(p => p.Id).ToHashSet();

            if (HasElementWithSameNonRootParents(parentIdSet))
            {
                Debug.LogWarning(
                    $"Cannot generate child because another element already has parents [{string.Join(", ", parentIdSet)}], " +
                    $"and only {{Root}} is allowed to have duplicate children.");
                return null;
            }
            
            // 2. Combine DNA and gather all effects
            List<Effect> effects = new List<Effect>();
            Seed newSeed = new Seed(0, -1, "GeneratedFromParents");
            foreach (Element parent in parentElements)
            {
                combineSeeds(newSeed, parent.GetDna());
                List<Effect> parentEffects = parent.GetEffects();
                if (parentEffects is { Count: > 0 })
                    effects.AddRange(parentEffects);
            }

            // 3. mutate and normalize
            newSeed = MutateSeed(newSeed, mutateType);
            newSeed.NormalizeSeedValue();
            ulong baseValue = newSeed.GetBaseValue();
            
            // 4. Determine effect types in newSeed
            List<Effect> childEffects = new();
            List<float> childValues = new();
            
            for (int bit = 0; bit < 64; bit++)  //ulong 
            {
                if (!newSeed.IsBitSet(bit))
                    continue;

                // Figure out what effect type this bit represents
                // (We can simply scan parent effects for unique types)
                Type effectType = allParentEffects
                    .Select(e => e.GetType())
                    .Distinct()
                    .ElementAtOrDefault(bit % allParentEffects.Select(e => e.GetType()).Distinct().Count()); //TODO: not divide by 0 ensure

                if (effectType == null)
                    continue;

                // All parent effects of that type
                var candidates = allParentEffects
                    .Where(e => e.GetType() == effectType)
                    .ToList();

                float[] values;

                if (candidates.Count > 0)
                {
                    // Multiple parents have same effect — deterministically pick one
                    int mode = (int)((baseValue >> (bit % 3)) & 0b11);
                    var chosen = finalEffectSelectorMode(mode, candidates);
                    values = chosen.Values;
                    childEffects.Add(chosen.Clone());
                    childValues.AddRange(values);
                }
                else
                {
                    // No parent had this effect: derive new deterministic floats
                    int paramCount = 2; // Default or get from reflection if needed
                    values = new float[paramCount];
                    ulong localSeed = (baseValue >> bit) ^ (baseValue << (bit % 8));
                    for (int p = 0; p < paramCount; p++)
                    {
                        values[p] = (float)((localSeed >> (p * 8)) & 0xFF) / 255f * 10f;
                    }

                    // Try to create an instance using reflection and pass the generated values
                    var instance = (Effect)Activator.CreateInstance(effectType, values);
                    childEffects.Add(instance);
                    childValues.AddRange(values);
                }
            }
            
            // 5. Fallback if seed produced no valid effects
            if (childEffects.Count == 0 && allParentEffects.Count > 0)
            {
                var fallback = fallbackEffectSelector(allParentEffects);
                if (fallback != null)
                {
                    childEffects.Add(fallback.Clone());
                    childValues.AddRange(fallback.Values);
                    Debug.LogWarning($"Seed {baseValue} produced no active bits — fallback used.");
                }
            }
            
            // 6. Optionally boost effects on crossing over
            if (operationLabel == "crossing over")
                ApplySeedBoosts(childEffects, baseValue);

            
            // 7. Construct final Element
            int currId = CountElements();
            var child = new Element($"CustomElement:{currId}", currId, newSeed, childValues.ToArray());

            // 8. Register in graph
            if (!registryGraph.ContainsVertex(child))
                registryGraph.AddVertex(child);

            foreach (var parent in parentElements)
            {
                if (!registryGraph.ContainsEdge(parent, child))
                    registryGraph.AddEdge(new Edge<Element>(parent, child));
            }

            Debug.Log(
                $"Generated new element '{child.Name}' (ID {currId}) with {childEffects.Count} effects " +
                $"from {operationLabel} parents [{string.Join(", ", parentElements.Select(p => p.Name))}] (Seed={baseValue})"
            );

            return child;
        }
        
        /// <summary>
        /// Returns true if some element (non-root) already has exactly the same parents,
        /// except the parent set { root } which is allowed to have duplicates.
        /// </summary>
        private bool HasElementWithSameNonRootParents(HashSet<int> parentIds)
        {
            // Allow duplicates if parents == { root }
            if (parentIds.Count == 1 && parentIds.Contains(rootElement.Id))
                return false;

            foreach (var vertex in registryGraph.Vertices)
            {
                // Skip root itself
                if (vertex == rootElement)
                    continue;

                var thisParents = registryGraph
                    .InEdges(vertex)
                    .Select(e => e.Source.Id)
                    .ToHashSet();

                // Skip elements whose only parent is root
                if (thisParents.Count == 1 && thisParents.Contains(rootElement.Id))
                    continue;

                // If another element has the same parent set → duplicate → not allowed
                if (thisParents.SetEquals(parentIds))
                    return true;
            }

            return false;
        }
    }
}
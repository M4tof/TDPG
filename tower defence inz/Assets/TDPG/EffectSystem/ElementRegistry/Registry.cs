using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Serialization;
using TDPG.EffectSystem.ElementLogic;
using TDPG.Generators.Seed;
using static TDPG.Generators.Seed.Genetic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TDPG.EffectSystem.ElementRegistry
{
    [Serializable]
    public class Registry
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
            rootElement = new Element("Root", 0, new Seed(0, 0, "Root"));

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
            var parentElements = registryGraph.Vertices.Where(v => parentsId.Contains(v.Id)).ToList();
            if (parentElements.Count == 0)
            {
                Debug.LogWarning($"No parent elements found for IDs [{string.Join(",", parentsId)}].");
                return false;
            }

            // Add new element if it's not already in the graph
            if (!registryGraph.ContainsVertex(newElement))
                registryGraph.AddVertex(newElement);

            // Create edges (parent → child)
            foreach (var parent in parentElements)
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
            var parentElements = registryGraph.Vertices.Where(v => parentsId.Contains(v.Id)).ToList();
            if (parentElements.Count == 0)
            {
                Debug.LogWarning($"No parent elements found for IDs [{string.Join(",", parentsId)}].");
                return null;
            }

            // Combine DNA and Gather all effects from parents
            List<Effect> effects = new List<Effect>();
            Seed newSeed = new Seed(0, -1, "GeneratedFromParents");
            foreach (var parent in parentElements)
            {
                newSeed += parent.GetDna();
                var parentEffects = parent.GetEffects();
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
            int currId = registryGraph.VertexCount;
            
            // Create new element  
            Element newElement = new Element($"CustomElement:{currId}", currId, finalEffects, newSeed);
            
            // Add to registry and link with parents
            if (!registryGraph.ContainsVertex(newElement))
                registryGraph.AddVertex(newElement);
            
            foreach (var parent in parentElements)
            {
                // Parent → Child
                if (!registryGraph.ContainsEdge(parent, newElement))
                    registryGraph.AddEdge(new Edge<Element>(parent, newElement));
            }

            
            Debug.Log($"Generated new element '{newElement.Name}' (ID {currId}) from parents [{string.Join(", ", parentElements.Select(p => p.Name))}]");
            return newElement;
        }

//Access Elements in registry
        public Element GetElement(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("GetElement called with null or empty name.");
                return null;
            }
            
            // Find all matches
            var matches = registryGraph.Vertices
                .Where(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                Debug.LogWarning($"No element found with name '{name}'.");
                return null;
            }

            if (matches.Count > 1)
            {
                Debug.LogError($"Multiple elements found with name '{name}' (IDs: {string.Join(", ", matches.Select(m => m.Id))}).");
                return null;
            }

            return matches[0];
        }
        public Element GetElement(int id)
        {
            // Find all elements with the given ID
            var matches = registryGraph.Vertices
                .Where(v => v.Id == id)
                .ToList();

            if (matches.Count == 0)
            {
                Debug.LogWarning($"No element found with ID {id}.");
                return null;
            }

            if (matches.Count > 1)
            {
                Debug.LogError($"Multiple elements found with ID {id} (Names: {string.Join(", ", matches.Select(m => m.Name))}).");
                return null;
            }

            return matches[0];
        }
        public Element GetElement(List<Element> parents) //Single child of multiple parents
        {
            if (parents == null || parents.Count == 0)
            {
                Debug.LogWarning("GetElement(List<Element>) called with null or empty parent list.");
                return null;
            }
            
            // Ensure all parent elements exist in the graph
            var validParents = parents.Where(p => registryGraph.ContainsVertex(p)).ToList();
            if (validParents.Count != parents.Count)
            {
                var missing = parents.Except(validParents).Select(p => p.Name);
                Debug.LogWarning($"Some parent elements not found in registry: [{string.Join(", ", missing)}]");
            }

            if (validParents.Count == 0)
            {
                Debug.LogWarning("No valid parents found in registry.");
                return null;
            }
            
            // Get children of the first parent
            var commonChildren = new HashSet<Element>(
                registryGraph.OutEdges(validParents[0]).Select(e => e.Target)
            );
            
            // Intersect with children of the rest
            for (int i = 1; i < validParents.Count; i++)
            {
                var children = registryGraph.OutEdges(validParents[i]).Select(e => e.Target);
                commonChildren.IntersectWith(children);
            }
            
            if (commonChildren.Count == 0)
            {
                Debug.Log($"No common child found for parents [{string.Join(", ", validParents.Select(p => p.Name))}].");
                return null;
            }

            if (commonChildren.Count > 1)
            {
                Debug.LogWarning($"Multiple common children found for parents [{string.Join(", ", validParents.Select(p => p.Name))}]: " +
                                 $"[{string.Join(", ", commonChildren.Select(c => c.Name))}]. Returning first one.");
            }

            return commonChildren.First();
        }
        public IEnumerable<Element> GetElementsFromParents(List<Element> parents)
        {
            if (parents == null || parents.Count == 0)
            {
                Debug.LogWarning("GetElementsFromParents called with null or empty parent list.");
                return Enumerable.Empty<Element>();
            }

            // Validate that all given parents exist in the registry
            var validParents = parents.Where(p => registryGraph.ContainsVertex(p)).ToList();
            if (validParents.Count != parents.Count)
            {
                var missing = parents.Except(validParents).Select(p => p.Name);
                Debug.LogWarning($"Some parent elements not found in registry: [{string.Join(", ", missing)}]");
            }

            if (validParents.Count == 0)
            {
                Debug.LogWarning("No valid parent elements found in registry.");
                return Enumerable.Empty<Element>();
            }

            // Get all children of the first valid parent
            var commonChildren = new HashSet<Element>(
                registryGraph.OutEdges(validParents[0]).Select(e => e.Target)
            );

            // Intersect with children of the other parents
            for (int i = 1; i < validParents.Count; i++)
            {
                var children = registryGraph.OutEdges(validParents[i]).Select(e => e.Target);
                commonChildren.IntersectWith(children);
            }

            if (commonChildren.Count == 0)
            {
                Debug.Log($"No common children found for parents [{string.Join(", ", validParents.Select(p => p.Name))}].");
                return Enumerable.Empty<Element>();
            }

            Debug.Log($"Found {commonChildren.Count} common child(ren) for parents [{string.Join(", ", validParents.Select(p => p.Name))}]: " +
                      $"[{string.Join(", ", commonChildren.Select(c => c.Name))}]");

            return commonChildren;
        }
        public IEnumerable<Element> GetDescendants(Element root, int maxDepth = 1)
        {
            if (root == null)
            {
                Debug.LogWarning("GetElements called with null root.");
                return Enumerable.Empty<Element>();
            }

            if (!registryGraph.ContainsVertex(root))
            {
                Debug.LogWarning($"Root element '{root.Name}' not found in registry.");
                return Enumerable.Empty<Element>();
            }

            if (maxDepth < 1)
            {
                Debug.LogWarning("GetElements called with maxDepth < 1. Returning empty list.");
                return Enumerable.Empty<Element>();
            }

            HashSet<Element> visited = new HashSet<Element> { root };
            HashSet<Element> descendants = new HashSet<Element>();

            // Queue stores (element, currentDepth)
            Queue<(Element element, int depth)> queue = new Queue<(Element, int)>();
            queue.Enqueue((root, 0));

            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();

                if (depth >= maxDepth)
                    continue;

                // Traverse all children (outgoing edges)
                foreach (var edge in registryGraph.OutEdges(current))
                {
                    var child = edge.Target;

                    if (!visited.Contains(child))
                    {
                        visited.Add(child);
                        descendants.Add(child);
                        queue.Enqueue((child, depth + 1));
                    }
                }
            }

            Debug.Log($"Found {descendants.Count} descendant(s) of '{root.Name}' up to depth {maxDepth}: " +
                      $"[{string.Join(", ", descendants.Select(e => e.Name))}]");

            return descendants;
        }

//Utils and Debug
        public void PrintRegistryMap()
        {
            Debug.Log("=== REGISTRY MAP ===");

            foreach (var element in registryGraph.Vertices)
            {
                // Get parent and child lists using OutEdges (children) and InEdges (parents)
                var parents = registryGraph.Edges
                    .Where(e => e.Target.Equals(element))
                    .Select(e => e.Source.Name)
                    .Distinct()
                    .ToList();

                var children = registryGraph.Edges
                    .Where(e => e.Source.Equals(element))
                    .Select(e => e.Target.Name)
                    .Distinct()
                    .ToList();

                string parentList = parents.Count > 0 ? string.Join(", ", parents) : "—";
                string childList = children.Count > 0 ? string.Join(", ", children) : "—";

                Debug.Log($"{element.Name} (ID {element.Id}) | Parents: [{parentList}] | Children: [{childList}]");
            }

            Debug.Log("=== END OF REGISTRY MAP ===");
        }

        private void PrintSubtree(Element element, int depth, HashSet<Element> visited) // Helper: recursively print the subtree structure (with visited set to avoid cycles)
        {
            if (element == null || visited.Contains(element))
                return;

            visited.Add(element);

            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}- {element.Name} (ID {element.Id})");

            // Children of this node (OutEdges: parent -> child)
            var children = registryGraph.OutEdges(element)
                .Select(e => e.Target)
                .Where(c => c != null)
                .Distinct()
                .ToList();

            foreach (var child in children)
            {
                PrintSubtree(child, depth + 1, visited);
            }
        }
        public void PrintElementDetails(string name)
        {
            Element tmpRead = GetElement(name);

            if (tmpRead == null)
            {
                Debug.LogWarning($"Element '{name}' not found in registry.");
                return;
            }
            
            // Get parents and children
            var parents = registryGraph.InEdges(tmpRead).Select(e => e.Source).Distinct().ToList();
            var children = registryGraph.OutEdges(tmpRead).Select(e => e.Target).Distinct().ToList();
            
            // Prepare readable strings
            string parentNames = parents.Count > 0 ? string.Join(", ", parents.Select(p => $"{p.Name} (ID {p.Id})")) : "None";
            string childNames = children.Count > 0 ? string.Join(", ", children.Select(c => $"{c.Name} (ID {c.Id})")) : "None";
            
            // Print element core info
            Debug.Log($"=== Element Details ===\n" +
                      $"Name: {tmpRead.Name}\n" +
                      $"ID: {tmpRead.Id}\n" +
                      $"Meta: {tmpRead.MetaData}\n" +
                      $"Parents: {parentNames}\n" +
                      $"Children: {childNames}\n" +
                      $"Seed: {tmpRead.GetDna()}\n" +
                      $"Effects: {string.Join(", ", tmpRead.GetEffects()?.Select(e => e.Name) ?? new List<string>())}\n" +
                      $"========================");
        }
        public void ClearRegistry()
        {
            registryGraph = new BidirectionalGraph<Element, Edge<Element>>(true);
            // re-add rootElement so API behaviour is consistent (root always present)
            if (rootElement != null)
                registryGraph.AddVertex(rootElement);
        }
        public int CountElements()
        {
            return registryGraph.VertexCount;
        }
        private Effect AverageEffects(List<Effect> effects)
        {
            if (effects == null || effects.Count == 0)
                return null;

            var baseEffect = effects[0];
            int paramCount = baseEffect.ParamNum();
            float[] averagedValues = new float[paramCount];

            // Average parameters across all effects
            for (int i = 0; i < paramCount; i++)
            {
                averagedValues[i] = effects.Average(e => e.GetValues()[i]);
            }

            Type effectType = baseEffect.GetType();

            // Try to find a constructor that matches the number of float parameters
            var constructor = effectType.GetConstructors()
                .FirstOrDefault(c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.All(p => p.ParameterType == typeof(float)) &&
                           parameters.Length == averagedValues.Length;
                });

            if (constructor != null)
            {
                // Pass each averaged float as a separate argument
                object[] args = averagedValues.Cast<object>().ToArray();
                try
                {
                    return (Effect)constructor.Invoke(args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to instantiate averaged {effectType.Name}: {ex.Message}");
                    return baseEffect;
                }
            }

            Debug.LogWarning($"No suitable constructor found for effect type {effectType.Name}. Returning first instance.");
            return baseEffect;
        }
        private void ApplySeedBoosts(List<Effect> effects, ulong seedValue)
        {
            if (effects == null || effects.Count == 0)
                return;

            ulong workingSeed = seedValue;

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                float[] values = effect.GetValues();

                for (int v = 0; v < values.Length; v++)
                {
                    // Take the next 2 seed digits (mod 100) to create a small boost
                    int seedChunk = (int)(workingSeed % 100);
                    workingSeed /= 100;

                    // Scale between 0.00 and 0.20 for subtle variation
                    float boost = 0.01f + (seedChunk / 100f) * 0.2f;

                    values[v] += boost;

                    // Clamp for sanity if needed (optional)
                    values[v] = Mathf.Clamp(values[v], 0f, 999f);
                }

                // Create a new boosted instance of the same effect type
                Effect boosted = effect.WithValues(values);
                effects[i] = boosted;
            }
        }
        
//Serialize Deserialize
        public static Registry Deserialize(string json)
        {
            //TODO: 
            return null;
        }
        public string Serialize(Registry registry)
        {
            //TODO: 
            return null;
        }
    }
}
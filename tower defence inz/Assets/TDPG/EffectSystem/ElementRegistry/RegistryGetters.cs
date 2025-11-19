using System;
using System.Collections.Generic;
using System.Linq;
using TDPG.EffectSystem.ElementLogic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementRegistry
{
    public partial class Registry
    {
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
        public IEnumerable<Element> GetElementsFromParents(List<Element> parents) //All kids of multiple parents
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
        
    }
}
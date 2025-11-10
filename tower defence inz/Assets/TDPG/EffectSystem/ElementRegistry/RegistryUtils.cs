using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using TDPG.EffectSystem.ElementLogic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementRegistry
{
    public partial class Registry
    {
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
        
    }
}
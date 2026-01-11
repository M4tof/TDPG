using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.EffectSystem.ElementLogic;
using TDPG.EffectSystem.ElementRegistry;
using UnityEngine;

namespace TDPG.EffectSystem.ElementPlanner
{
    public class ElementPlanner
    {
        private readonly Registry registry;
        private readonly List<Element> activeElements = new();
        private readonly List<IEffectAction> plannedActions = new();

        public ElementPlanner(Registry registry)
        {
            this.registry = registry;
        }

        public void RegisterElement(string elementName)
        {
            var el = registry.GetElement(elementName); //todo: look up id
            if (el == null) throw new Exception($"Element '{elementName}' not found.");
            activeElements.Add(el);
            Debug.Log("Registered Element: " + el.Name);
        }

        public void RegisterElement(string elementName, float[] values, bool overwrite_values = false)
        {
            var el = registry.GetElement(elementName); //todo: look up id
            if (el == null) throw new Exception($"Element '{elementName}' not found.");
            el.effects[0].Values = values;
            activeElements.Add(el);
            Debug.Log("Registered Element: " + el.Name);
        }

        public void BuildPlan()
        {
            plannedActions.Clear();

            foreach (var element in activeElements)
            {
                foreach (var effect in element.GetEffects())
                {
                    var map = effect.LogicTransfer();

                    foreach (var kv in map)
                    {
                        var action = EffectActionFactory.Create(kv.Key, kv.Value);
                        plannedActions.Add(action);
                    }
                }
            }
        }

        public void ExecutePlan(EffectContext context)
        {
            Debug.Log($"Execute Plan: {plannedActions.Count.ToString()}");
            foreach (var action in plannedActions)
            {
                Debug.Log($"Execute Plan: {action.Name}");
                action.Execute(context);
            }
            Debug.Log("End Execute Plan");
        }

        public IReadOnlyList<IEffectAction> GetPlannedActions()
        {
            // Najbezpieczniej zwracać *read-only snapshot*
            return plannedActions.AsReadOnly();
        }

    }
}

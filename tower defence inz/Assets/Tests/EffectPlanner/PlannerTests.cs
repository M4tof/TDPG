using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.EffectSystem.ElementLogic;
using TDPG.EffectSystem.ElementPlanner;
using TDPG.EffectSystem.ElementRegistry;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.EffectPlanner
{
    [TestFixture,Category("Planner")]
    
    public class EffectPlannerTests
    {

        private EffectContext CreateContext(GameObject target = null)
        {
            return new EffectContext
            {
                Attacker = new GameObject("Attacker"),
                Target = target,
                HitPosition = Vector3.zero,
                Grid = null
            };
        }

        [Test]

        public void Registry_ReturnsElementByName()
        {
            var registry = new Registry();
            var fire = new Element("Fire", 1, new Seed(1, -1, "Fire"));
            registry.PutPreMadeElement(new List<int> { 0 }, fire);

            var result = registry.GetElement("Fire");

            Assert.IsNotNull(result);
            Assert.AreEqual("Fire", result.Name);
        }

        [Test]
        public void Planner_BuildPlan_GeneratesCorrectOrderOfActions()
        {
            var registry = new Registry();
            var planner = new ElementPlanner(registry);

            var slow = new SlowDown(0.2f, 3f);
            var dmg = new HealthDown(5f);

            var element = new Element("Fire", 1, new List<Effect> { slow, dmg });
            registry.PutPreMadeElement(new List<int> { 0 }, element);
            planner.RegisterElement("Fire");

            // Build
            planner.BuildPlan();

            var plan = planner.GetPlannedActions().ToList();

            Assert.AreEqual(3, plan.Count);

            Assert.IsInstanceOf<SlowDownAction>(plan[0]);
            Assert.IsInstanceOf<DurationAction>(plan[1]);
            Assert.IsInstanceOf<HealthDownAction>(plan[2]);
        }


        [Test]
        public void SlowDownAction_Execution_ReducesSpeedCorrectly()
        {
            var target = new GameObject("Enemy");
            var stats = target.AddComponent<EnemyStats>();

            var action = new SlowDownAction(0.3f); // 30% slow
            var ctx = CreateContext(target);

            action.Execute(ctx);

            Assert.AreEqual(7f, stats.Speed); // 10 * (1 - 0.3)
        }

        [Test]
        public void HealthDownAction_Execution_DecreasesHealth()
        {
            var target = new GameObject("Enemy");
            var stats = target.AddComponent<EnemyStats>();

            var action = new HealthDownAction(-15f);
            var ctx = CreateContext(target);

            action.Execute(ctx);

            Assert.AreEqual(85f, stats.Health);
        }

        [Test]
        public void HealAction_Execution_IncreasesHealth()
        {
            var target = new GameObject("Ally");
            var stats = target.AddComponent<AllyStats>();

            var action = new HealAction(20f);
            var ctx = CreateContext(target);

            action.Execute(ctx);

            Assert.AreEqual(70f, stats.Health);
        }

        [Test]
        public void DurationAction_Execution_AddsDurationEntry()
        {
            var target = new GameObject("Enemy");
            var effects = target.AddComponent<EnemyStatusEffects>();

            var action = new DurationAction(4f);
            var ctx = CreateContext(target);

            action.Execute(ctx);

            Assert.AreEqual(1, effects.Durations.Count);
            Assert.AreEqual(4f, effects.Durations[0]);
        }

        [Test]
        public void Planner_MultipleElements_ProducesSequentialActions()
        {
            var registry = new Registry();
            var planner = new ElementPlanner(registry);

            var el1 = new Element("Fire", 1, new List<Effect>
            {
                new SlowDown(0.1f, 2f) // Slow + Duration
            });

                    var el2 = new Element("Poison", 2, new List<Effect>
            {
                new HealthDown(3f)     // HealthDown
            });

            registry.PutPreMadeElement(new List<int> { 0 }, el1);
            registry.PutPreMadeElement(new List<int> { 1 }, el2);

            planner.RegisterElement("Fire");
            planner.RegisterElement("Poison");

            planner.BuildPlan();

            var actions = planner.GetPlannedActions().ToList();

            Assert.AreEqual(3, actions.Count);
            Assert.IsInstanceOf<SlowDownAction>(actions[0]);
            Assert.IsInstanceOf<DurationAction>(actions[1]);
            Assert.IsInstanceOf<HealthDownAction>(actions[2]);
        }

        [Test]
        public void Planner_ExecutePlan_ExecutesAllActions()
        {
            var registry = new Registry();
            var planner = new ElementPlanner(registry);

            var element = new Element("Combo", 1, new List<Effect>
            {
                new SlowDown(0.2f, 1f),
                new HealthDown(5f)
            });

            registry.PutPreMadeElement(new List<int> { 0 }, element);

            planner.RegisterElement("Combo");
            planner.BuildPlan();

            var target = new GameObject("Enemy");
            target.AddComponent<EnemyStats>();

            var context = CreateContext(target);

            planner.ExecutePlan(context);

            var stats = target.GetComponent<EnemyStats>();

            Assert.Less(stats.Speed, 10f);
            Assert.Less(stats.Health, 100f);
        }

        [Test]
        public void Planner_ElementWithoutEffects_DoesNothing()
        {
            var registry = new Registry();
            var planner = new ElementPlanner(registry);

            var emptyElement = new Element("Empty", 1, new List<Effect>());
            registry.PutPreMadeElement(new List<int> { 0 }, emptyElement);
            planner.RegisterElement("Empty");

            planner.BuildPlan();

            Assert.AreEqual(0, planner.GetPlannedActions().Count());
        }


        [Test]

        public void Planner_NoTarget_DoesNotThrow()
        {
            var registry = new Registry();
            var planner = new ElementPlanner(registry);

            

            registry.PutPreMadeElement(new List<int> { 0 }, new Element("Fire", 1, new List<Effect> { new SlowDown(0.2f, 2f) }));

            planner.RegisterElement("Fire");

            planner.BuildPlan();

            var ctx = new EffectContext
            {
                Attacker = new GameObject("Attacker"),
                Target = null
            };

            Assert.DoesNotThrow(() => planner.ExecutePlan(ctx));
        }

        [Test]
        public void Planner_LogicTransfer_ProducesExpectedActions()
        {
            var registry = new Registry();
            var planner = new ElementPlanner(registry);

            var effect = new SlowDown(0.4f, 5f);
            var element = new Element("Ice", 2, new List<Effect> { effect });

            registry.PutPreMadeElement(new List<int> { 0 }, element);
            
            planner.RegisterElement("Ice");
            planner.BuildPlan();

            var actions = planner.GetPlannedActions().ToList();

            Assert.AreEqual(2, actions.Count);
            Assert.IsInstanceOf<SlowDownAction>(actions[0]);
            Assert.IsInstanceOf<DurationAction>(actions[1]);
        }

    }

}

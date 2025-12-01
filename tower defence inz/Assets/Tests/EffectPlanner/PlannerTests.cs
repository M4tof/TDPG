using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.EffectSystem.ElementLogic;
using TDPG.EffectSystem.ElementPlanner;
using TDPG.EffectSystem.ElementRegistry;
using TDPG.Templates.Enemies;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.EffectPlanner
{
    public class EnemyClass : EnemyBase
    {
        // Saved Data
        public string EnemyID; // "Goblin"
        public EnemyStatsOverride Overrides;

        // Runtime Only (Re-assigned on Load)
        [System.NonSerialized] private EnemyData _baseData;

        // Movement State
        private Queue<Vector2> _path;
        private Vector2? _currentTarget;

        public EnemyClass(EnemyData baseData, EnemyStatsOverride overrides) : base(baseData)
        {
            _baseData = baseData;
            EnemyID = baseData.EnemyName;
            Overrides = overrides;

            // Initialize State
            CurrentHealth = baseData.MaxHealth * overrides.HealthMultiplier;
            CurrentSpeed = baseData.Speed * overrides.SpeedMultiplier;
            // Position set by Spawner...
        }

        // Called after loading from JSON to reconnect the SO
        public void RestoreReference(EnemyData data)
        {
            _baseData = data;
        }

        // TODO: double check
        public void SetPath(IEnumerable<Vector2> pathPoints)
        {
            _path = new Queue<Vector2>(pathPoints);
            if (_path.Count > 0)
            {
                Position = _path.Peek(); // Snap to start
                GetNextTarget();
            }
        }

        public override void OnUpdate()
        {
            // DeltaTime handling: Logic usually assumes a fixed step or needs DT passed in.
            // If OnUpdate() doesn't take float dt, we assume Time.deltaTime (which breaks strict lib separation)
            // OR we change the signature in EnemyBase to OnUpdate(float deltaTime).
            Move(Time.deltaTime);
        }

        private void Move(float deltaTime)
        {
            if (_currentTarget == null) return;

            // Move towards target
            float step = CurrentSpeed * deltaTime;
            Position = Vector2.MoveTowards(Position, _currentTarget.Value, step);

            // Check if reached
            if (Vector2.Distance(Position, _currentTarget.Value) < 0.01f)
            {
                GetNextTarget();
            }
        }
        private void GetNextTarget()
        {
            if (_path != null && _path.Count > 0)
                _currentTarget = _path.Dequeue();
            else
                _currentTarget = null; // Reached end of path
        }
    };

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

        /*[Test]
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
        }*/

        /*
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
        }*/

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

        /*[Test]
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
        }*/

        [Test]
        public void Planner_ExecutePlan_ExecutesAllActions()
        {
            var registry = new Registry();
            var planner = new ElementPlanner(registry);

            var element = new Element("Combo", 1, new List<Effect>
            {
                new TempSlowDown(0.2f, 1f),
                new HealthDown(5f)
            });

            registry.PutPreMadeElement(new List<int> { 0 }, element);

            planner.RegisterElement("Combo");
            planner.BuildPlan();

            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.EnemyName = "Orc";
            data.MaxHealth = 120;
            data.Speed = 3.5f;
            data.EnemySprite = null; // lub wczytany sprite
            
            var overrides = new EnemyStatsOverride
            {
                HealthMultiplier = 1.0f,
                SpeedMultiplier = 1.0f
            };

            EnemyClass enemy = new EnemyClass(data, overrides);


            var target = new GameObject("Enemy");

            // 2. Dodajemy wymagane komponenty
            target.AddComponent<SpriteRenderer>();
            var behaviour = target.AddComponent<EnemyBaseBehaviour>();
            behaviour.Initialize(enemy);

            var context = CreateContext(target);

            planner.ExecutePlan(context);

            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var beh))
            {
                Assert.Less(beh.Logic.CurrentHealth, 120.0f);
            }
            else
            {
                Assert.Fail();
            }
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

            

            registry.PutPreMadeElement(new List<int> { 0 }, new Element("Fire", 1, new List<Effect> { new TempSlowDown(0.2f, 2f) }));

            planner.RegisterElement("Fire");

            planner.BuildPlan();

            var ctx = new EffectContext
            {
                Attacker = new GameObject("Attacker"),
                Target = null
            };

            Assert.DoesNotThrow(() => planner.ExecutePlan(ctx));
        }
        /*
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
        }*/

    }

}

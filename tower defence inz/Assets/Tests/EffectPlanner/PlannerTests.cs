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
        public string EnemyID;
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
            CurrentDamage = baseData.Damage;
            CurrentAttackSpeed = baseData.AttackSpeed;
        }

        // Called after loading from JSON to reconnect the SO
        public void RestoreReference(EnemyData data)
        {
            _baseData = data;
        }
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

    }

}

using System.Collections;
using System.Collections.Generic;
using TDPG.Generators.AttackPatterns;
using TDPG.Templates.Grid;
using UnityEngine;

namespace TDPG.Templates.Turret
{
    /// <summary>
    /// The concrete runtime implementation of an active Turret.
    /// <br/>
    /// Handles the combat loop: detecting enemies within range, selecting the best target 
    /// (currently "Closest"), and instantiating projectiles based on <see cref="TurretData"/>.
    /// </summary>
    public class Turret : TurretBase
    {
        [Header("Runtime State")] [SerializeField]
        private Transform shootPosition;
        private float _cooldownTimer;
        private Transform _currentTarget;
        private int _enemyLayerMask;

        protected override void Awake()
        {
            _enemyLayerMask = LayerMask.GetMask("Enemy");
            EnsureOffsetsCached();
            if (shootPosition == null)
            {
                shootPosition = transform;
            }
        }

        void Update()
        {
            if (Data == null) return;

            // Cooldown Management
            if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

            // Combat Loop
            if (_cooldownTimer <= 0)
            {
                PerformCombatLoop();
            }
        }

        /// <summary>
        /// Orchestrates the firing sequence: Acquire Targets -> Select Best -> Fire.
        /// </summary>
        private void PerformCombatLoop()
        {
            List<Transform> candidates = GetPossibleTargets();
            _currentTarget = SelectTarget(candidates);

            if (_currentTarget != null)
            {
                // Check if the pattern generator is set
                if (Data.PatternGenerator != null)
                {
                    // Start only if the previous pattern is over
                    // or according to the FireRate
                    if (_cooldownTimer <= 0)
                    {
                        ExecutePattern(_currentTarget);
                        _cooldownTimer = 1f / Data.FireRate;
                    }
                }
                else
                {
                    // A typical shot
                    Shoot(_currentTarget);
                    _cooldownTimer = 1f / Data.FireRate;
                }
            }
        }

        private void ExecutePattern(Transform target)
        {
            // Generate a new pattern (using eg. the time as the seed for the uniqueness)
            var pattern = Data.PatternGenerator.Preview((ulong)(Time.time * 1000));
            StartCoroutine(PatternRoutine(pattern, target));
        }

        private IEnumerator PatternRoutine(AttackPattern pattern, Transform target)
        {
            float startTime = Time.time;
            int eventIndex = 0;

            // Sort by the time, so as not to confuse the chronology
            pattern.events.Sort((a, b) => a.timeOffset.CompareTo(b.timeOffset));

            while (eventIndex < pattern.events.Count)
            {
                float elapsed = Time.time - startTime;

                if (elapsed >= pattern.events[eventIndex].timeOffset)
                {
                    SpawnProjectileFromEvent(pattern.events[eventIndex], target);
                    eventIndex++;
                }
                yield return null; // Wait until the following frame
            }
        }

        private void SpawnProjectileFromEvent(AttackEvent ev, Transform target)
        {
            if (Data.ProjectilePrefab == null) return;

            // 1. Calculate the base rotation towards the target
            Vector3 targetDir = (target != null) ? (target.position - transform.position).normalized : transform.right;
            float baseRotZ = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

            // 2. Apply pattern modificators
            float finalRotZ = baseRotZ + ev.spreadAngle;
            Quaternion bulletRotation = Quaternion.Euler(0f, 0f, finalRotZ);

            Vector3 spawnPos = shootPosition.position;

            GameObject bulletGo = Instantiate(Data.ProjectilePrefab, spawnPos, bulletRotation);

            Projectile projectile = bulletGo.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.damage = Data.Damage;
                foreach(CardData element in GetPlayerCardApplied())
                {
                    if (element.elementName != "")
                    {
                        projectile.AddElement(element.elementName);
                    }
                }
            }
        }

        /// <summary>
        /// Performs a Physics OverlapCircle check to find all colliders on the "Enemy" layer within range.
        /// <br/>
        /// <b>Note:</b> Range is scaled by <see cref="GridManager.CellSize"/> to ensure consistency across different grid scales.
        /// </summary>
        /// <returns>A list of valid enemy transforms.</returns>
        private List<Transform> GetPossibleTargets()
        {
            List<Transform> validTargets = new List<Transform>();

            float cellSize = (GridManager.Instance != null) ? GridManager.Instance.CellSize : 1.0f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Data.Range * cellSize, _enemyLayerMask);

            foreach (var hit in hits)
            {
                validTargets.Add(hit.transform);
            }
            return validTargets;
        }

        /// <summary>
        /// Evaluates a list of candidates and picks the optimal target based on a strategy.
        /// <br/>
        /// <b>Current Strategy:</b> Closest Distance.
        /// </summary>
        /// <param name="candidates">List of enemies in range.</param>
        /// <returns>The best target, or null if list is empty.</returns>
        private Transform SelectTarget(List<Transform> candidates)
        {
            if (candidates.Count == 0) return null;

            Transform bestTarget = null;
            float closestDistSqr = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            foreach (var candidate in candidates)
            {
                if (candidate == null) continue;

                float dSqr = (candidate.position - currentPos).sqrMagnitude;
                if (dSqr < closestDistSqr)
                {
                    closestDistSqr = dSqr;
                    bestTarget = candidate;
                }
            }
            return bestTarget;
        }

        /// <summary>
        /// Instantiates the projectile defined in <see cref="TurretData.ProjectilePrefab"/>, 
        /// aligns it towards the target, and resets the cooldown.
        /// </summary>
        /// <param name="target">The locked target.</param>
        private void Shoot(Transform target)
        {
            if (Data.ProjectilePrefab == null) return;

            // Calculate the base rotation towards the target
            Vector3 direction = (target.position - transform.position).normalized;
            float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion bulletRotation = Quaternion.Euler(0f, 0f, rotZ);

            // Spawn Projectile
            Vector3 spawnPos = shootPosition.position;

            GameObject projectileObject = Instantiate(Data.ProjectilePrefab, spawnPos, bulletRotation);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.damage = Data.Damage;
                foreach (CardData element in GetPlayerCardApplied())
                {
                    if (element.elementName != "")
                    {
                        projectile.AddElement(element.elementName);
                    }
                }
            }
        }
    }
}
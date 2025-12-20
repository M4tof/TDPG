using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Grid;

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
        [Header("Runtime State")]
        private float _cooldownTimer;
        private Transform _currentTarget;
        private int _enemyLayerMask;

        protected override void Awake()
        {
            _enemyLayerMask = LayerMask.GetMask("Enemy");
            EnsureOffsetsCached();
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
            // Step 1: Get Possible Targets (Range + Visibility)
            List<Transform> candidates = GetPossibleTargets();

            // Step 2: Select Target (Strategy)
            _currentTarget = SelectTarget(candidates);

            /*foreach (var item in candidates)
            {
                Debug.Log($"target in sight: {item}");
            }*/

            // Step 3: Shoot
            if (_currentTarget != null)
            {
                Shoot(_currentTarget);
                _cooldownTimer = 1f / Data.FireRate;
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

            // A. Range Check (Physics)
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Data.Range * cellSize, _enemyLayerMask);

            foreach (var hit in hits)
            {
                // B. Line of Sight Check (Optional/Placeholder)
                // If you add walls later, perform a Raycast here.
                if (CheckLineOfSight(hit.transform))
                {
                    validTargets.Add(hit.transform);
                }
            }
            return validTargets;
        }

        private bool CheckLineOfSight(Transform target)
        {
            // Placeholder: Simply return true for now.
            // Future implementation: Raycast from Turret center to Target center. 
            // If it hits "Wall" layer before "Enemy", return false.
            return true;
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

            // Strategy: CLOSEST (Default)
            // TODO: Refactor this into a Strategy Pattern (e.g., ITargetingStrategy)

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

            // Calculate rotation towards target
            Vector3 direction = (target.position - transform.position).normalized;
            float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion bulletRotation = Quaternion.Euler(0f, 0f, rotZ);

            // Spawn Projectile
            // Use center of tile (transform.position) + offset if needed
            Vector3 spawnPos = transform.position + (Vector3)(Data.TileSize * 0.5f);

            Instantiate(Data.ProjectilePrefab, spawnPos, bulletRotation);

            // Note: We stop here. 
            // The BasicProjectile script takes over movement via its FixedUpdate.
            // Damage application is handled by the projectile's collision logic (not implemented here).
        }

    }
}

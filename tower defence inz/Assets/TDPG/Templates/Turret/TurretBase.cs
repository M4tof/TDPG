using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Grid;

namespace TDPG.Templates.Turret
{
    /// <summary>
    /// Abstract base class representing a placed Turret entity in the game world.
    /// <br/>
    /// Handles visual initialization (Sprite assignment, Scaling) and state management using <see cref="TurretData"/>.
    /// </summary>
    public abstract class TurretBase : MonoBehaviour
    {

        [Header("Runtime State")]
        [Tooltip("The configuration data (Stats, Sprites, Size) driving this specific turret instance.")]
        public TurretData Data;
        
        [Header("Visuals")]
        [SerializeField] [Tooltip("Renderer for the static base/pedestal of the turret.")] public SpriteRenderer baseRenderer;
        [SerializeField] [Tooltip("Renderer for the active element/crystal of the turret.")] public SpriteRenderer crystalRenderer;

        private Vector3 _baseDesignPos;
        private Vector3 _crystalDesignPos;
        private Vector3 _baseDesignScale;
        private Vector3 _crystalDesignScale;
        private bool _initializedOffsets = false;


        protected virtual void Awake()
        {
            EnsureOffsetsCached();
        }

        /// <summary>
        /// Captures the initial LocalPosition and LocalScale of the renderers.
        /// <br/>
        /// This creates a "Design Baseline" so that procedural scaling (via <see cref="Initialize"/>) 
        /// is always applied relative to the prefab's original layout, rather than compounding over time.
        /// </summary>
        protected void EnsureOffsetsCached()
        {
            if (_initializedOffsets) return;

            if (baseRenderer != null)
            {
                _baseDesignPos = baseRenderer.transform.localPosition;
                _baseDesignScale = baseRenderer.transform.localScale; // Capture Scale
            }

            if (crystalRenderer != null)
            {
                _crystalDesignPos = crystalRenderer.transform.localPosition;
                _crystalDesignScale = crystalRenderer.transform.localScale; // Capture Scale
            }
            _initializedOffsets = true;
        }
        
        /// <summary>
        /// Configures the turret instance with specific gameplay data.
        /// <br/>
        /// Updates sprites and applies procedural scaling based on <see cref="TurretData.Multiplayer"/>.
        /// </summary>
        /// <param name="data">The data container defining this turret's properties.</param>
        public virtual void Initialize(TurretData data)
        {
            EnsureOffsetsCached(); // Safety check
            Data = data;

            // Safety: Default to 1.0 if GridManager is missing (e.g. prefab mode)
            float cellSize = (GridManager.Instance != null) ? GridManager.Instance.CellSize : 1.0f;

            // Calculate the specific offset to center this turret on its tiles
            // Vector3 gridCenterOffset = new Vector3(
            //     data.TileSize.x * cellSize * 0.5f,
            //     data.TileSize.y * cellSize * 0.5f,
            //     0f
            // );

            // APPLY: Always set relative to the CLEAN Design Position
            if (baseRenderer != null)
            {
                baseRenderer.sprite = data.BaseSprite;
                baseRenderer.transform.localScale = new Vector3(
                    _baseDesignScale.x * data.Multiplayer,
                    _baseDesignScale.y * data.Multiplayer,
                    1f
                );
                Vector3 scaledDesignPos = new Vector3(
                    _baseDesignPos.x * data.Multiplayer,
                    _baseDesignPos.y * data.Multiplayer,
                    _baseDesignPos.z
                );

                baseRenderer.transform.localPosition = scaledDesignPos;
            }

            if (crystalRenderer != null)
            {
                crystalRenderer.sprite = data.CrystalSprite;
                crystalRenderer.transform.localScale = new Vector3(
                    _crystalDesignScale.x * data.Multiplayer,
                    _crystalDesignScale.y * data.Multiplayer,
                    1f
                );
                Vector3 scaledDesignPos = new Vector3(
                    _crystalDesignPos.x * data.Multiplayer,
                    _crystalDesignPos.y * data.Multiplayer,
                    _crystalDesignPos.z
                );

                crystalRenderer.transform.localPosition = scaledDesignPos;

            }
        }
        
        /// <summary>
        /// Returns the grid dimensions of the turret (Width x Height).
        /// </summary>
        public Vector2 GetTileSize() => Data != null ? Data.TileSize : Vector2.one;
        
        /// <summary>
        /// Returns the unique ID from the associated data.
        /// </summary>
        public string GetTurretID() => Data != null ? Data.TurretID : "";

        void OnDrawGizmosSelected()
        {
            if (Data != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, Data.Range);
            }
        }

    }
}

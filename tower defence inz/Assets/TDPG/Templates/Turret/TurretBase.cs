using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Grid;
using UnityEngine.Events;

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

        [Tooltip("Modyficator for Turret")]
        [SerializeField] List<CardData> playerCardApplied = new List<CardData>();

        [Tooltip("The current health points of the turret.")]
        private int currentHealth;

        [Header("Visuals")]
        [SerializeField] [Tooltip("Renderer for the static base/pedestal of the turret.")] public SpriteRenderer baseRenderer;
        [SerializeField] [Tooltip("Renderer for the active element/crystal of the turret.")] public SpriteRenderer crystalRenderer;

        public UnityEvent turretDestroyed;
        public UnityEvent HealthChanged;
        
        private TurretData BaseData;
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
            /*if (_initializedOffsets) return;

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
            _initializedOffsets = true;*/
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
            
            Data = Instantiate(data);           //make copy of data
            SetTurretBaseData(data);            //Make copy of data to save Base stat
            currentHealth = data.MaxHP;
            SetOffset(data);
            transform.localScale = new Vector3(data.Scale, data.Scale, 0);
        }

        public void SetOffset(TurretData data)
        {
            if (baseRenderer != null)
            {
                baseRenderer.sprite = data.BaseSprite;
                baseRenderer.transform.localPosition = data.BaseOffset;
            }

            if (crystalRenderer != null)
            {
                crystalRenderer.sprite = data.CrystalSprite;
                crystalRenderer.transform.localPosition = data.CrystalOffset;
                Debug.Log($"OFFSET CRYSTAL {data.CrystalOffset} vs {crystalRenderer.transform.localPosition}");
            }
            Debug.Log("OFFSET END");
        }

        /// <summary>
        /// Applies damage to the turret and checks for destruction conditions.
        /// </summary>
        /// <param name="damage">Amount of damage to apply.</param>
        public void DealDamage(int damage)
        {
            Debug.Log("ATTACK TURRET");
            currentHealth -= damage;
            HealthChanged.Invoke();
            if (currentHealth <= 0)
            {
                DestroyTurret();
            }
        }

        /// <summary>
        /// Handles the destruction of the turret, updating the grid and removing the GameObject.
        /// </summary> 
        public void DestroyTurret()
        {
            float cellSize = GridManager.Instance.GetCellSize();
            for (int i = 0; i < Data.TileSize.x; i++)
            {
                for (int j = 0; j < Data.TileSize.y; j++)
                {
                    GridManager.Instance.SetTileType(transform.position + new Vector3(i*cellSize,j*cellSize,0),Grid.Grid.TileType.EMPTY);
                }
            }
            //GridManager.Instance.SetTileType(transform.position,Grid.Grid.TileType.EMPTY);
            turretDestroyed.Invoke();
            Destroy(gameObject);
        }


        /// <summary>
        /// Returns the grid dimensions of the turret (Width x Height).
        /// </summary>
        public Vector2 GetTileSize() => Data != null ? Data.TileSize : Vector2.one;
        
        /// <summary>
        /// Returns the unique ID from the associated data.
        /// </summary>
        public string GetTurretID() => Data != null ? Data.TurretID : "";
        
        /// <summary>
        /// Returns the current HP of this turret for cost calculations.
        /// </summary>
        public int GetCurrentHealth() => currentHealth;

        /// <summary>
        /// Apply all modifiers from list and set list of modifiers
        /// </summary>
        public void ApplyModifiers(List<CardData> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                Debug.Log("SET Modifier are EMPTY!!!");
                return;
            } 
            Debug.Log("SET Modifier");
            foreach (CardData modifier in modifiers)
            {
                ApplyModifier(modifier);
                playerCardApplied.Add(modifier);
            }
        }
        
        /// <summary>
        /// Apply modifiers which changes parameters of turret
        /// </summary>
        public void ApplyModifier(CardData modifier)
        {
            if (modifier.hpMultiplayer != 1)
            {
                Data.MaxHP += Mathf.RoundToInt(BaseData.MaxHP * (modifier.hpMultiplayer-1));
                currentHealth += Data.MaxHP - currentHealth;
                HealthChanged.Invoke();
            }
            if (modifier.damageMultiplayer != 1)
            {
                Data.Damage += Mathf.RoundToInt(BaseData.Damage * (modifier.damageMultiplayer-1));
            }
            if (modifier.rangeMultiplayer != 1)
            {
                Data.Range += Mathf.RoundToInt(BaseData.Range * (modifier.rangeMultiplayer-1));
            }
            if(modifier.PatternGenerator != null)
            {
                Data.PatternGenerator = modifier.PatternGenerator;
            }

            Data.Cost += modifier.ResourceCost;
        }


        /// <summary>
        /// Add and apply modifier to current modifiers
        /// </summary>
        public void AddAndApplyModifier(CardData modifier)
        {
            Debug.Log("SET Modifier");
            if (modifier == null) 
            {
                return;
            }
            ApplyModifier(modifier);

            playerCardApplied.Add(modifier);
        }

        /// <summary>
        /// Getter for Data
        /// </summary>
        public TurretData GetData()
        {
            return Data;
        }

        /// <summary>
        /// Get id from Data
        /// </summary>
        public string GetTurretId()
        {
            return Data.TurretID;
        }
        
        /// <summary>
        /// Set Copy of Data to BaseData
        /// </summary>
        public void SetTurretBaseData(TurretData data)
        {
            BaseData = Instantiate(data);
        }
        
        /// <summary>
        /// Get copy of BaseData
        /// </summary>
        public TurretData GetTurretBaseData()
        {
            return Instantiate(BaseData);
        }

        public List<CardData> GetPlayerCardApplied()
        {
            return playerCardApplied;
        }
        
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

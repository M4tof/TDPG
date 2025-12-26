using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    /// <summary>
    /// Shared base for controllers targeting the 'ColorSwapMultipleNoAlpha.shader'.
    /// Handles MaterialPropertyBlock management, deterministic palette selection, and blink effects.
    /// </summary>
    [ExecuteAlways]
    public abstract class BaseColorSwapController : MonoBehaviour, IColorSwapController
    {
        [Header("Procedural Logic")]
        [Tooltip("Optional: The helper that chooses a palette based on a Seed.")]
        [SerializeField] protected ChoseAndApplyPalette paletteSelector;

        [Header("Target Override")]
        [Tooltip("Manual override for the palette. If set, this takes priority over the Procedural Logic.")]
        [SerializeField] protected ColorPaletteSO activePalette;

        [Header("Effects")]
        [Tooltip("The duration (in seconds) of the white flash effect triggered by BlinkWhite.")]
        [SerializeField] protected float blinkDuration = 0.1f;

        /// <summary> Cached renderer component. </summary>
        protected Renderer _renderer;
        /// <summary> Cached property block to avoid GC allocations. </summary>
        protected MaterialPropertyBlock _propBlock;
        /// <summary> Stores colors passed via SetPalette(List<Color>) at runtime. </summary>
        protected List<Color> _runtimePaletteOverrides;
        /// <summary> Reference to the active blink coroutine. </summary>
        protected Coroutine _blinkCoroutine;

        // --- Shader Property IDs ---
        /// <summary> ID for the number of active color swaps. </summary>
        protected static readonly int CountID = Shader.PropertyToID("_Count");
        /// <summary> Array of IDs for original detection colors (with tolerance in alpha). </summary>
        protected static readonly int[] OrigIDs = new int[16];
        /// <summary> Array of IDs for the replacement colors. </summary>
        protected static readonly int[] TargIDs = new int[16];

        /// <summary>
        /// Static constructor to initialize shader IDs once.
        /// </summary>
        static BaseColorSwapController()
        {
            for (int i = 0; i < 16; i++)
            {
                OrigIDs[i] = Shader.PropertyToID($"_Orig{i}");
                TargIDs[i] = Shader.PropertyToID($"_Targ{i}");
            }
        }

        /// <summary> Initializes the shader properties when enabled. </summary>
        protected virtual void OnEnable() => UpdateShaderProperties();

        /// <summary> Updates shader properties when inspector values change. </summary>
        protected virtual void OnValidate()
        {
            // If the user manually changes the activePalette asset, clear runtime overrides
            if (activePalette != null) _runtimePaletteOverrides = null;
            UpdateShaderProperties();
        }

        // -----------------------------------------------------------------------
        // INTERFACE IMPLEMENTATION
        // -----------------------------------------------------------------------

        /// <summary> Sets a specific single target color at index 0. </summary>
        public virtual void SetTargetColor(Color color)
        {
            if (_runtimePaletteOverrides == null) _runtimePaletteOverrides = new List<Color>();
            if (_runtimePaletteOverrides.Count == 0) _runtimePaletteOverrides.Add(color);
            else _runtimePaletteOverrides[0] = color;
            activePalette = null;
            UpdateShaderProperties();
        }

        /// <summary> Sets target colors from a raw list of colors. </summary>
        public virtual void SetPalette(List<Color> colors)
        {
            _runtimePaletteOverrides = new List<Color>(colors);
            activePalette = null;
            UpdateShaderProperties();
        }

        /// <summary> Sets the active target palette from a ScriptableObject. </summary>
        public virtual void SetPalette(ColorPaletteSO palette)
        {
            activePalette = palette;
            _runtimePaletteOverrides = null;
            UpdateShaderProperties();
        }

        /// <summary> Triggers the white flash effect. </summary>
        public void BlinkWhite()
        {
            if (!Application.isPlaying) return;
            if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
            if (gameObject.activeInHierarchy) _blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        /// <summary> Logic for the blink coroutine. </summary>
        private System.Collections.IEnumerator BlinkRoutine()
        {
            ApplyToRenderer(true);
            yield return new WaitForSeconds(blinkDuration);
            ApplyToRenderer(false);
            _blinkCoroutine = null;
        }

        // -----------------------------------------------------------------------
        // CORE LOGIC
        // -----------------------------------------------------------------------

        /// <summary> Refreshes the shader properties on the renderer. </summary>
        public void UpdateShaderProperties() => ApplyToRenderer(false);

        /// <summary> Internal method to resolve the palette and push data to the GPU. </summary>
        private void ApplyToRenderer(bool forceWhite)
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propBlock);

            // Resolve which target colors to use
            List<Color> currentTargets = ResolveTargetColors();

            // Hand off to specific controller implementation
            OnUpdateBlock(_propBlock, currentTargets, forceWhite);

            _renderer.SetPropertyBlock(_propBlock);
        }

        /// <summary>
        /// Priority-based resolution of target colors.
        /// 1. Runtime list overrides, 2. Inspector Palette SO, 3. Seed-based selection.
        /// </summary>
        private List<Color> ResolveTargetColors()
        {
            if (_runtimePaletteOverrides != null) return _runtimePaletteOverrides;
            if (activePalette != null) return activePalette.colors;
            if (paletteSelector != null)
            {
                ColorPaletteSO chosen = paletteSelector.ChosePalette();
                if (chosen != null) return chosen.colors;
            }
            return null;
        }

        /// <summary> Implemented by children to handle their specific list structures. </summary>
        protected abstract void OnUpdateBlock(MaterialPropertyBlock block, List<Color> resolvedTargets, bool forceWhite);
    }
}
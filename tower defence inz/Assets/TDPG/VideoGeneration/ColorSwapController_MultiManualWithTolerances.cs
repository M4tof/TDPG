using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    /// <summary>
    /// Controller used by the TDPG library to provide procedural visual modification via color swapping.
    /// <br/>
    /// It must be placed on the same GameObject as an <see cref="UnityEngine.SpriteRenderer"/>.
    /// <br/>
    /// This version of the controller works only with <c>TDPG_MultiColorTolerance.mat</c> that implements <c>ColorSwapMultipleNoAlpha.shader</c>.
    /// <br/>
    /// This variant supports multiple original colors (one target color per original).  <br/>
    /// The tolerance setting is per each color, they can overlap and the first in order is going to be drawn.
    /// </summary>
    [ExecuteAlways]
    public class ColorSwapController_MultiManualWithTolerances : MonoBehaviour, IColorSwapController
    {
        [System.Serializable]
        public class ColorSwapEntry
        {
            [Tooltip("A descriptive name for this specific color swap (e.g., 'Skin', 'Armor').")]
            public string name = "Swap";
            
            // 1. Hide Alpha in Inspector because we use that channel for logic, not transparency
            [Tooltip("The specific color in the source texture to be replaced. NOTE: Alpha is disabled for the sake of transferring tolerance.")]
            [ColorUsage(false, false)] 
            public Color original = Color.white;
            
            // 2. Add per-swap tolerance
            [Range(0f, 1f)] 
            [Tooltip("The threshold for color matching. Higher values will replace a wider range of colors similar to the Original Color. Applied to this swap.")] 
            public float tolerance = 0.05f; 
            
            [Tooltip("The new color that will replace the Original Color.")]
            public Color target = Color.red;
        }
        
        [Header("Effects")]
        [Tooltip("The duration (in seconds) of the white flash effect triggered by BlinkWhite.")] 
        [SerializeField] private float blinkDuration = 0.1f;
        private Coroutine _blinkCoroutine;
        
        [Header("Swaps (Max 16)")]
        [Tooltip("Swap objects, used to hold the swap from original color to target color.")]
        public List<ColorSwapEntry> swaps = new List<ColorSwapEntry>();

        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        // IDs
        private static readonly int CountID = Shader.PropertyToID("_Count");
        // Removed ToleranceID
        
        // Dynamic ID Arrays
        private static readonly int[] OrigIDs;
        private static readonly int[] TargIDs;

        static ColorSwapController_MultiManualWithTolerances()
        {
            OrigIDs = new int[16];
            TargIDs = new int[16];
            for (int i = 0; i < 16; i++)
            {
                OrigIDs[i] = Shader.PropertyToID($"_Orig{i}");
                TargIDs[i] = Shader.PropertyToID($"_Targ{i}");
            }
        }

        void OnEnable() => UpdateShaderProperties();
        void OnValidate() => UpdateShaderProperties();

        // -----------------------------------------------------------------------
        // INTERFACE IMPLEMENTATION
        // -----------------------------------------------------------------------
        public void SetTargetColor(Color color)
        {
            if (swaps.Count > 0 && swaps[0] != null)
            {
                swaps[0].target = color;
                UpdateShaderProperties();
            }
        }

        public void SetPalette(List<Color> colors)
        {
            if (colors == null) return;
            for (int i = 0; i < swaps.Count && i < colors.Count; i++)
            {
                if (swaps[i] != null) swaps[i].target = colors[i];
            }
            UpdateShaderProperties();
        }

        public void SetPalette(ColorPaletteSO palette)
        {
            if (palette != null && palette.colors != null) SetPalette(palette.colors);
        }

        public void SetTargetColorAtIndex(int index, Color color)
        {
            if (index >= 0 && index < swaps.Count && swaps[index] != null)
            {
                swaps[index].target = color;
                UpdateShaderProperties();
            }
        }
        
        public void BlinkWhite()
        {
            // Coroutines only work in Play Mode usually, so we check Application.isPlaying
            if (!Application.isPlaying) return;

            if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);

            if (gameObject.activeInHierarchy)
            {
                _blinkCoroutine = StartCoroutine(BlinkRoutine());
            }
        }

        private System.Collections.IEnumerator BlinkRoutine()
        {
            // --- STEP 1: Setup ---
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) yield break;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_propBlock);

            int count = Mathf.Min(swaps.Count, 16);
            _propBlock.SetInt(CountID, count);

            // --- STEP 2: Loop through swaps and set Targets to WHITE ---
            for (int i = 0; i < count; i++)
            {
                ColorSwapEntry entry = swaps[i];
                if (entry != null)
                {
                    // CRITICAL FIX: Pack the tolerance into the alpha channel
                    Color packedOrig = entry.original;
                    packedOrig.a = entry.tolerance; 

                    _propBlock.SetColor(OrigIDs[i], packedOrig);
                    
                    // Force Target to White
                    _propBlock.SetColor(TargIDs[i], Color.white);
                }
            }

            _renderer.SetPropertyBlock(_propBlock);

            // --- STEP 3: Wait ---
            yield return new WaitForSeconds(blinkDuration);

            // --- STEP 4: Revert ---
            UpdateShaderProperties();
            _blinkCoroutine = null;
        }
        
        // -----------------------------------------------------------------------
        // UPDATE
        // -----------------------------------------------------------------------
        /// <summary>
        /// Applies the current values given to the controller, either via code or inspector.
        /// </summary>
        public void UpdateShaderProperties()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propBlock);
            // Removed _propBlock.SetFloat(ToleranceID...)
            
            int count = Mathf.Min(swaps.Count, 16);
            _propBlock.SetInt(CountID, count);

            for (int i = 0; i < count; i++)
            {
                ColorSwapEntry entry = swaps[i];
                if (entry != null)
                {
                    // 3. PACKING: Combine Color RGB with Tolerance Alpha
                    Color packedOrig = entry.original;
                    packedOrig.a = entry.tolerance;

                    _propBlock.SetColor(OrigIDs[i], packedOrig);
                    _propBlock.SetColor(TargIDs[i], entry.target);
                }
            }

            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
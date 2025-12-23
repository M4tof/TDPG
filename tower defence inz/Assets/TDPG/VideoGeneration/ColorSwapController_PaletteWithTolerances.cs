using System.Collections.Generic;
using TDPG.Generators.Seed;
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
    /// This variant supports multiple original colors and uses a set palette as target colors. <br/>
    /// The tolerance setting is per each color, they can overlap and the first in order is going to be drawn.
    /// </summary>
    [ExecuteAlways]
    public class ColorSwapController_PaletteWithTolerances : MonoBehaviour, IColorSwapController
    {
         // 1. Define a struct to hold Color + Tolerance
        [System.Serializable]
        public struct ColorSwapEntry
        {
            [ColorUsage(false, false)] // Disable alpha picker in Inspector to avoid confusion
            [Tooltip("Original color on the sprite, pre shading")]
            public Color color;
            
            [Range(0, 1.0f)] 
            [Tooltip("The threshold for color matching. Higher values will replace a wider range of colors similar to the Original Color. Applied to this swap.")] 
            public float tolerance;
        }
        
        [Header("Effects")]
        [Tooltip("The duration (in seconds) of the white flash effect triggered by BlinkWhite.")] 
        [SerializeField] private float blinkDuration = 0.1f;
        private Coroutine _blinkCoroutine;

        [Header("Original Colors (Max 16)")]
        [Tooltip("Define the specific colors on the sprite you want to replace and their specific tolerance.")]
        public List<ColorSwapEntry> originalColors = new List<ColorSwapEntry>();

        [Header("Procedural Generation")]
        [Tooltip("If Active Palette is empty, it will choose one from here using the Seed.")]
        [SerializeField] private AllowedPalettesSO allowedPalettes;
        [SerializeField] private Seed seed;
        
        [Header("Target Palette")]
        [Tooltip("The ScriptableObject containing the target colors.")]
        [SerializeField] private ColorPaletteSO activePalette;

        private List<Color> _runtimePaletteOverrides;
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        // IDs
        private static readonly int CountID = Shader.PropertyToID("_Count");
        // ToleranceID removed, we don't need it globally anymore
        private static readonly int[] OrigIDs;
        private static readonly int[] TargIDs;

        static ColorSwapController_PaletteWithTolerances()
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
        
        void OnValidate() 
        {
            if (activePalette != null) _runtimePaletteOverrides = null;
            UpdateShaderProperties();
        }

        private ColorPaletteSO GetDeterministicPalette()
        {
            if (allowedPalettes == null || allowedPalettes.palettes == null || allowedPalettes.palettes.Count == 0)
                return null;

            if (seed == null) return null;

            int count = allowedPalettes.palettes.Count;
            ulong rawVal = seed.GetBaseValue();

            // Calculate how many digits we need (9 = 1, 12 = 2, 105 = 3, etc)
            int digitsNeeded = 1;
            if (count > 1)
                digitsNeeded = Mathf.FloorToInt(Mathf.Log10(count - 1)) + 1;

            // Extract the digits via power of 10
            long powerOf10 = (long)Mathf.Pow(10, digitsNeeded);
            ulong extractedDigits = rawVal % (ulong)powerOf10;

            // Map to valid index (Modulo ensures it's within 0 to count-1)
            int finalIndex = (int)(extractedDigits % (ulong)count);

            return allowedPalettes.palettes[finalIndex];
        }
        
        // -----------------------------------------------------------------------
        // INTERFACE IMPLEMENTATION
        // -----------------------------------------------------------------------
        public void SetTargetColor(Color color)
        {
            if (_runtimePaletteOverrides == null) _runtimePaletteOverrides = new List<Color>();
            if (_runtimePaletteOverrides.Count == 0) _runtimePaletteOverrides.Add(color);
            else _runtimePaletteOverrides[0] = color;

            activePalette = null;
            UpdateShaderProperties();
        }

        public void SetPalette(List<Color> colors)
        {
            _runtimePaletteOverrides = new List<Color>(colors);
            activePalette = null;
            UpdateShaderProperties();
        }

        public void SetPalette(ColorPaletteSO newPalette)
        {
            activePalette = newPalette;
            _runtimePaletteOverrides = null;
            UpdateShaderProperties();
        }
        
        public void BlinkWhite()
        {
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

            int count = Mathf.Min(originalColors.Count, 16);
            _propBlock.SetInt(CountID, count);

            // --- STEP 2: Loop through ORIGINAL COLORS and set Targets to WHITE ---
            for (int i = 0; i < count; i++)
            {
                ColorSwapEntry entry = originalColors[i];
                
                Color packedOrig = entry.color;
                packedOrig.a = entry.tolerance;

                _propBlock.SetColor(OrigIDs[i], packedOrig);
                _propBlock.SetColor(TargIDs[i], Color.white);
            }
            _renderer.SetPropertyBlock(_propBlock);

            // --- STEP 3: Wait ---
            yield return new WaitForSeconds(blinkDuration);

            // --- STEP 4: Revert ---
            UpdateShaderProperties();
            _blinkCoroutine = null;
        }


        // -----------------------------------------------------------------------
        // LOGIC
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
            
            int count = Mathf.Min(originalColors.Count, 16);
            _propBlock.SetInt(CountID, count);

            // RESOLVE PALETTE:
            // 1. Priority: Runtime code-set overrides
            // 2. Priority: Manually assigned Active Palette
            // 3. Priority: Procedural selection via Seed
            List<Color> currentTargets = null;

            if (_runtimePaletteOverrides != null) 
            {
                currentTargets = _runtimePaletteOverrides;
            }
            else 
            {
                // If activePalette is null, try to choose one procedurally
                ColorPaletteSO paletteToUse = activePalette;
                if (paletteToUse == null)
                {
                    paletteToUse = GetDeterministicPalette();
                }

                if (paletteToUse != null)
                {
                    currentTargets = paletteToUse.colors;
                }
            }

            for (int i = 0; i < count; i++)
            {
                ColorSwapEntry entry = originalColors[i];
                Color packedOrig = entry.color;
                packedOrig.a = entry.tolerance; 

                Color targ = entry.color; 
                if (currentTargets != null && i < currentTargets.Count)
                {
                    targ = currentTargets[i];
                }

                _propBlock.SetColor(OrigIDs[i], packedOrig);
                _propBlock.SetColor(TargIDs[i], targ);
            }

            _renderer.SetPropertyBlock(_propBlock);
        }
        
        
        
    }
}
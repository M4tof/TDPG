using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    [ExecuteAlways]
    public class ColorSwapController_PaletteWithTolerances : MonoBehaviour, IColorSwapController
    {
         // 1. Define a struct to hold Color + Tolerance
        [System.Serializable]
        public struct ColorSwapEntry
        {
            [ColorUsage(false, false)] // Disable alpha picker in Inspector to avoid confusion
            public Color color;
            
            [Range(0, 1.0f)] 
            public float tolerance;
        }
        
        [Header("Effects")]
        [SerializeField] private float blinkDuration = 0.1f;
        private Coroutine _blinkCoroutine;

        [Header("Original Colors (Max 16)")]
        [Tooltip("Define the specific colors on the sprite you want to replace and their specific tolerance.")]
        // 2. Change list to use the new struct
        public List<ColorSwapEntry> originalColors = new List<ColorSwapEntry>();

        [Header("Target Palette")]
        [Tooltip("The ScriptableObject containing the replacement colors.")]
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
        public void UpdateShaderProperties()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propBlock);
            // No global tolerance set
            
            int count = Mathf.Min(originalColors.Count, 16);
            _propBlock.SetInt(CountID, count);

            List<Color> currentTargets = null;
            if (_runtimePaletteOverrides != null) currentTargets = _runtimePaletteOverrides;
            else if (activePalette != null) currentTargets = activePalette.colors;

            for (int i = 0; i < count; i++)
            {
                ColorSwapEntry entry = originalColors[i];
                
                // 3. PACKING:
                // We take the RGB from the inspector color.
                // We take the Tolerance float and put it in the Alpha channel.
                Color packedOrig = entry.color;
                packedOrig.a = entry.tolerance; 

                Color targ = entry.color; 
                // Note: The target doesn't need tolerance, so we just use the color.
                // We keep alpha 1.0 (or whatever the target is) for the result.

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
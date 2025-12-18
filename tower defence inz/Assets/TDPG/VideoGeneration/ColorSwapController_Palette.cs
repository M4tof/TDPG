using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TDPG.VideoGeneration
{
    /// <summary>
    /// Controller used by the TDPG library to provide procedural visual modification via color swapping.
    /// <br/>
    /// It must be placed on the same GameObject as an <see cref="UnityEngine.SpriteRenderer"/>.
    /// <br/>
    /// This version of the controller works only with <c>TDPG_MultiColor.mat</c> that implements <c>ColorSwapMultiple.shader</c>.
    /// <br/>
    /// This variant supports multiple original colors and uses a set palette as target colors. The tolerance setting is global and applies to all defined swaps.
    /// </summary>
    [ExecuteAlways]
    public class ColorSwapController_Palette : MonoBehaviour, IColorSwapController
    {
        [Header("Settings")]
        [Tooltip("The threshold for color matching. Higher values will replace a wider range of colors similar to the Original Color. Applied globally to all swaps.")] 
        [Range(0, 10)] public float tolerance = 0.05f;
        
        [Header("Effects")]
        [Tooltip("The duration (in seconds) of the white flash effect triggered by BlinkWhite.")] 
        [SerializeField] private float blinkDuration = 0.1f;
        private Coroutine _blinkCoroutine;
        
        [Header("Original Colors (Max 16)")]
        [Tooltip("Define the specific colors on the sprite you want to replace.")]
        public List<Color> originalColors = new List<Color>();

        [Header("Target Palette")]
        [Tooltip("The ScriptableObject containing the replacement target colors.")]
        [SerializeField] private ColorPaletteSO activePalette;

        private List<Color> _runtimePaletteOverrides;
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        // IDs
        private static readonly int CountID = Shader.PropertyToID("_Count");
        private static readonly int ToleranceID = Shader.PropertyToID("_Tolerance");
        private static readonly int[] OrigIDs;
        private static readonly int[] TargIDs;

        static ColorSwapController_Palette()
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

            _propBlock.SetFloat(ToleranceID, tolerance);

            int count = Mathf.Min(originalColors.Count, 16);
            _propBlock.SetInt(CountID, count);

            // --- STEP 2: Loop through swaps and set Targets to WHITE ---
            for (int i = 0; i < count; i++)
            {
                _propBlock.SetColor(OrigIDs[i], originalColors[i]);
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
            _propBlock.SetFloat(ToleranceID, tolerance);
            
            int count = Mathf.Min(originalColors.Count, 16);
            _propBlock.SetInt(CountID, count);

            List<Color> currentTargets = null;
            if (_runtimePaletteOverrides != null) currentTargets = _runtimePaletteOverrides;
            else if (activePalette != null) currentTargets = activePalette.colors;

            for (int i = 0; i < count; i++)
            {
                Color orig = originalColors[i];
                Color targ = orig; 

                if (currentTargets != null && i < currentTargets.Count)
                {
                    targ = currentTargets[i];
                }

                _propBlock.SetColor(OrigIDs[i], orig);
                _propBlock.SetColor(TargIDs[i], targ);
            }

            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
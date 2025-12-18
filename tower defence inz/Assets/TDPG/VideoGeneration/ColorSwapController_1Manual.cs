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
    /// This version of the controller works only with <c>TDPG_1DominantMat.mat</c> that implements <c>ColorSwap1.shader</c>.
    /// <br/>
    /// This variant has one original color and one target color; it is the simplest version of the TDPG palette swapping.
    /// </summary>
    [ExecuteAlways] // Updates in Edit Mode so you can see changes instantly
    public class ColorSwapController_1Manual : MonoBehaviour, IColorSwapController
    {
        [Header("Settings")] 
        [Tooltip("The specific color in the source texture to be replaced.")]
        [SerializeField] private Color originalColor = Color.white;
        
        [Tooltip("The new color that will replace the Original Color.")] 
        [SerializeField] private Color targetColor = Color.red;
        
        [Tooltip("The threshold for color matching. Higher values will replace a wider range of colors similar to the Original Color.")] 
        [Range(0, 10)] public float tolerance = 0.01f;

        [Header("Effects")]
        [Tooltip("The duration (in seconds) of the white flash effect triggered by BlinkWhite.")] 
        [SerializeField] private float blinkDuration = 0.1f;
        
        private Coroutine _blinkCoroutine;
        
        // Cache the Renderer and PropertyBlock to avoid garbage collection
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        // Cache shader property IDs for performance
        private static readonly int OriginalColorID = Shader.PropertyToID("_OriginalColor");
        private static readonly int TargetColorID = Shader.PropertyToID("_TargetColor");
        private static readonly int ToleranceID = Shader.PropertyToID("_Tolerance");

        void OnEnable()
        {
            UpdateColor();
        }

        void OnValidate()
        {
            // Called whenever you change a value in the Inspector
            UpdateColor();
        }

        /// <summary>
        /// Applies the current Original Color, Target Color, and Tolerance settings to the Renderer's MaterialPropertyBlock.
        /// </summary>
        public void UpdateColor()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;

            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            // 1. Get the current state of the block
            _renderer.GetPropertyBlock(_propBlock);

            // 2. Set the new values
            _propBlock.SetColor(OriginalColorID, originalColor);
            _propBlock.SetColor(TargetColorID, targetColor);
            _propBlock.SetFloat(ToleranceID, tolerance);

            // 3. Apply the block back to the renderer
            _renderer.SetPropertyBlock(_propBlock);
        }
        
        // -----------------------------------------------------------------------
        // INTERFACE IMPLEMENTATION (Hot Swapping)
        // -----------------------------------------------------------------------

        public void SetTargetColor(Color color)
        {
            targetColor = color;
            UpdateColor();
        }

        public void SetPalette(List<Color> colors)
        {
            if (colors != null && colors.Count > 0)
            {
                // This controller only supports 1 color, so we take the first one
                targetColor = colors[0];
                UpdateColor();
            }
        }

        public void SetPalette(ColorPaletteSO palette)
        {
            if (palette != null && palette.colors != null && palette.colors.Count > 0)
            {
                // This controller only supports 1 color, so we take the first one
                targetColor = palette.colors[0];
                UpdateColor();
            }
        }
        
        public void BlinkWhite()
        {
            // If we are already blinking, stop the previous one so we can restart (spam-click friendly)
            if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
    
            if (gameObject.activeInHierarchy)
            {
                _blinkCoroutine = StartCoroutine(BlinkRoutine());
            }
        }
        
        private System.Collections.IEnumerator BlinkRoutine()
        {
            // --- STEP 1: Turn White ---
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
    
            _renderer.GetPropertyBlock(_propBlock);

            // Keep the original selection logic, but force the output to White
            _propBlock.SetColor(OriginalColorID, originalColor);
            _propBlock.SetColor(TargetColorID, Color.white); // <--- Override to White
            _propBlock.SetFloat(ToleranceID, tolerance);
            _renderer.SetPropertyBlock(_propBlock);

            // --- STEP 2: Wait ---
            yield return new WaitForSeconds(blinkDuration); 

            // --- STEP 3: Revert ---
            UpdateColor();
    
            _blinkCoroutine = null;
        }
    }
}
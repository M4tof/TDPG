using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TDPG.VideoGeneration
{
    [ExecuteAlways]
    public class ColorSwapController_MultiManual : MonoBehaviour, IColorSwapController
    {
        [System.Serializable]
        public class ColorSwapEntry
        {
            public string name = "Swap";
            public Color original = Color.white;
            public Color target = Color.red;
        }

        [Header("Settings")]
        [Range(0, 10)] public float tolerance = 0.05f;
        
        [Header("Effects")]
        [SerializeField] private float blinkDuration = 0.1f;
        private Coroutine _blinkCoroutine;
        
        [Header("Swaps (Max 16)")]
        public List<ColorSwapEntry> swaps = new List<ColorSwapEntry>();

        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        // IDs
        private static readonly int CountID = Shader.PropertyToID("_Count");
        private static readonly int ToleranceID = Shader.PropertyToID("_Tolerance");
        
        // Dynamic ID Arrays
        private static readonly int[] OrigIDs;
        private static readonly int[] TargIDs;

        // Static Constructor to generate IDs once (Performance Optimization)
        static ColorSwapController_MultiManual()
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

            int count = Mathf.Min(swaps.Count, 16);
            _propBlock.SetInt(CountID, count);

            // --- STEP 2: Loop through swaps and set Targets to WHITE ---
            for (int i = 0; i < count; i++)
            {
                if (swaps[i] != null)
                {
                    // Keep the original detection color
                    _propBlock.SetColor(OrigIDs[i], swaps[i].original);
                    // Override the output color to WHITE
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
        public void UpdateShaderProperties()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(ToleranceID, tolerance);
            
            // Limit to 16
            int count = Mathf.Min(swaps.Count, 16);
            _propBlock.SetInt(CountID, count);

            for (int i = 0; i < count; i++)
            {
                if (swaps[i] != null)
                {
                    _propBlock.SetColor(OrigIDs[i], swaps[i].original);
                    _propBlock.SetColor(TargIDs[i], swaps[i].target);
                }
            }

            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
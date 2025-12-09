using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TDPG.VideoGeneration
{
    [ExecuteAlways]
    public class ColorSwapController_Palette : MonoBehaviour
    {
        [Header("Settings")]
        [Range(0, 10)] public float tolerance = 0.05f;

        [Header("Original Colors (Max 8)")]
        [Tooltip("Define the specific colors on the sprite you want to replace.")]
        public List<Color> originalColors = new List<Color>();

        [Header("Target Palette")]
        [Tooltip("The ScriptableObject containing the replacement colors.")]
        [SerializeField] private ColorPaletteSO activePalette;

        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        // IDs
        private static readonly int CountID = Shader.PropertyToID("_Count");
        private static readonly int ToleranceID = Shader.PropertyToID("_Tolerance");
        
        private static readonly int[] OrigIDs = {
            Shader.PropertyToID("_Orig0"), Shader.PropertyToID("_Orig1"),
            Shader.PropertyToID("_Orig2"), Shader.PropertyToID("_Orig3"),
            Shader.PropertyToID("_Orig4"), Shader.PropertyToID("_Orig5"),
            Shader.PropertyToID("_Orig6"), Shader.PropertyToID("_Orig7")
        };
        private static readonly int[] TargIDs = {
            Shader.PropertyToID("_Targ0"), Shader.PropertyToID("_Targ1"),
            Shader.PropertyToID("_Targ2"), Shader.PropertyToID("_Targ3"),
            Shader.PropertyToID("_Targ4"), Shader.PropertyToID("_Targ5"),
            Shader.PropertyToID("_Targ6"), Shader.PropertyToID("_Targ7")
        };

        void OnEnable() => UpdateShaderProperties();
        void OnValidate() => UpdateShaderProperties();

        /// <summary>
        /// Call this at runtime to hot-swap the color theme.
        /// </summary>
        public void SetPalette(ColorPaletteSO newPalette)
        {
            activePalette = newPalette;
            UpdateShaderProperties();
        }

        public void UpdateShaderProperties()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propBlock);

            _propBlock.SetFloat(ToleranceID, tolerance);
            
            // Limit to 8 because shader only supports 8
            int count = Mathf.Min(originalColors.Count, 8);
            _propBlock.SetInt(CountID, count);

            for (int i = 0; i < count; i++)
            {
                Color orig = originalColors[i];
                Color targ = orig; // Default to no change if palette is missing/empty

                if (activePalette != null && activePalette.colors != null)
                {
                    // If palette has a color for this index, use it. 
                    // Otherwise keep original.
                    if (i < activePalette.colors.Count)
                    {
                        targ = activePalette.colors[i];
                    }
                }

                _propBlock.SetColor(OrigIDs[i], orig);
                _propBlock.SetColor(TargIDs[i], targ);
            }

            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
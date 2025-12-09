using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TDPG.VideoGeneration
{
    [ExecuteAlways]
    public class ColorSwapController_MultiManual : MonoBehaviour
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
        
        [Header("Swaps (Max 8)")]
        public List<ColorSwapEntry> swaps = new List<ColorSwapEntry>();

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

        public void UpdateShaderProperties()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propBlock);

            _propBlock.SetFloat(ToleranceID, tolerance);
            
            // Limit to 8
            int count = Mathf.Min(swaps.Count, 8);
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
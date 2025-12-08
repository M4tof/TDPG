using UnityEngine;
using Color = UnityEngine.Color;

namespace TDPG.VideoGeneration
{
    [ExecuteAlways] // Updates in Edit Mode so you can see changes instantly
    public class ColorSwapController : MonoBehaviour
    {
        [Header("Settings")] 
        public Color originalColor = Color.white;
        public Color targetColor = Color.red;
        [Range(0, 10)] public float tolerance = 0.01f;

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
    }
}
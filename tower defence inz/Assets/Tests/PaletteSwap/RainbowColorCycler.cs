using TDPG.VideoGeneration;
using UnityEngine;

namespace Tests.PaletteSwap
{
    public class RainbowColorCycler : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("How fast the rainbow cycles. Higher = Faster.")]
        public float speed = 0.5f;

        [Tooltip("Intensity of the color (0 = Grey, 1 = Full Color).")]
        [Range(0f, 1f)] public float saturation = 1.0f;

        [Tooltip("Brightness of the color (0 = Black, 1 = Full Brightness).")]
        [Range(0f, 1f)] public float brightness = 1.0f;

        private IColorSwapController _controller;

        void Start()
        {
            _controller = GetComponent<IColorSwapController>();

            if (_controller == null)
            {
                Debug.LogError($"[RainbowColorCycler] No IColorSwapController found on '{name}'! " +
                               "Please attach a ColorSwapController script (e.g., 1Manual).", this);
                enabled = false;
            }
        }

        void Update()
        {
            if (_controller == null) return;

            // Calculate Hue based on time. 
            // Mathf.Repeat ensures the value loops smoothly from 0.0 to 1.0
            float hue = Mathf.Repeat(Time.time * speed, 1.0f);

            // Convert HSV to standard RGB Color
            Color rainbowColor = Color.HSVToRGB(hue, saturation, brightness);

            // Apply the color
            _controller.SetTargetColor(rainbowColor);
        }
    }
}
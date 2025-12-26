using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    /// <summary>
    /// Swaps multiple original colors with specifically defined target colors.
    /// Supports deterministic palette overrides via seed.
    /// </summary>
    public class ColorSwapControllerManual : BaseColorSwapController
    {
        /// <summary> Data structure for a full manual swap definition. </summary>
        [System.Serializable]
        public class ManualSwapEntry
        {
            [Tooltip("Friendly name for inspector organization.")]
            public string name = "Swap";
            [ColorUsage(false, false)]
            [Tooltip("Original color on the sprite.")]
            public Color original = Color.white;
            [Range(0f, 1f)]
            [Tooltip("Tolerance for this specific match.")]
            public float tolerance = 0.05f;
            [Tooltip("The color to use if no palette/procedural override is active.")]
            public Color target = Color.red;
        }

        [Header("Manual Setup")]
        [Tooltip("List of manual color swaps.")]
        public List<ManualSwapEntry> swaps = new List<ManualSwapEntry>();

        /// <summary> Helper to set a manual target color at a specific index via code. </summary>
        public void SetTargetColorAtIndex(int index, Color color)
        {
            if (index >= 0 && index < swaps.Count && swaps[index] != null)
            {
                swaps[index].target = color;
                UpdateShaderProperties();
            }
        }

        protected override void OnUpdateBlock(MaterialPropertyBlock block, List<Color> resolvedTargets, bool forceWhite)
        {
            int count = Mathf.Min(swaps.Count, 16);
            block.SetInt(CountID, count);

            for (int i = 0; i < count; i++)
            {
                ManualSwapEntry entry = swaps[i];
                if (entry == null) continue;

                // Pack tolerance into Alpha
                Color packedOrig = entry.original;
                packedOrig.a = entry.tolerance;

                // Selection Priority: 
                // 1. Force White (Blink), 2. Resolved Palette (Seed/SO/Runtime), 3. Manual Entry Target
                Color target;
                if (forceWhite) target = Color.white;
                else if (resolvedTargets != null && i < resolvedTargets.Count) target = resolvedTargets[i];
                else target = entry.target;

                block.SetColor(OrigIDs[i], packedOrig);
                block.SetColor(TargIDs[i], target);
            }
        }
    }
}
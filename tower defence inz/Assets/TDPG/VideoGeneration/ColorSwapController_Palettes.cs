using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    /// <summary>
    /// Swaps multiple original colors with colors provided by a deterministic palette or seed.
    /// </summary>
    public class ColorSwapControllerPalettes : BaseColorSwapController
    {
        /// <summary> Data structure for defining a detection color and its tolerance. </summary>
        [System.Serializable]
        public struct ColorSwapEntry
        {
            [ColorUsage(false, false)]
            [Tooltip("Original color on the sprite to be detected.")]
            public Color original;
            [Range(0, 1.0f)]
            [Tooltip("Tolerance for this specific color match.")]
            public float tolerance;
        }

        [Header("Detection Setup")]
        [Tooltip("The list of original colors to find and replace.")]
        public List<ColorSwapEntry> originalColors = new List<ColorSwapEntry>();

        protected override void OnUpdateBlock(MaterialPropertyBlock block, List<Color> resolvedTargets, bool forceWhite)
        {
            int count = Mathf.Min(originalColors.Count, 16);
            block.SetInt(CountID, count);

            for (int i = 0; i < count; i++)
            {
                ColorSwapEntry entry = originalColors[i];
                
                // Pack tolerance into Alpha
                Color packedOrig = entry.original;
                packedOrig.a = entry.tolerance;

                // Determine target color (Default to original if no palette resolved)
                Color target = entry.original;
                if (forceWhite) target = Color.white;
                else if (resolvedTargets != null && i < resolvedTargets.Count) target = resolvedTargets[i];

                block.SetColor(OrigIDs[i], packedOrig);
                block.SetColor(TargIDs[i], target);
            }
        }
    }
}
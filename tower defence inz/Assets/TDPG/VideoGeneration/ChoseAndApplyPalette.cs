using TDPG.Generators.Seed;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    [System.Serializable]
    public class ChoseAndApplyPalette
    {
        [Tooltip("The seed source for deterministic selection")]
        public Seed seed;
        
        [Tooltip("The list of possible palettes")]
        public AllowedPalettesSO allowedPalettes;

        public ColorPaletteSO ChosePalette()
        {
            if (allowedPalettes == null || allowedPalettes.palettes == null || allowedPalettes.palettes.Count == 0 || seed == null)
            {
                return null;
            }

            int count = allowedPalettes.palettes.Count;
            ulong rawVal = seed.GetBaseValue();

            // Deterministic selection via digit extraction
            int digitsNeeded = 1;
            if (count > 1)
            {
                digitsNeeded = Mathf.FloorToInt(Mathf.Log10(count - 1)) + 1;
            }

            long powerOf10 = (long)Mathf.Pow(10, digitsNeeded);
            ulong extractedDigits = rawVal % (ulong)powerOf10;

            // Map the extracted digits to the actual list count
            int finalIndex = (int)(extractedDigits % (ulong)count);

            return allowedPalettes.palettes[finalIndex];
        }
        
    }
}
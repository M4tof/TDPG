using TDPG.Generators.Seed;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    public class ChoseAndApplyPalette
    {
        [SerializeField] public Seed Seed;
        [SerializeField] public AllowedPalettesSO palettesSO;

        public ColorPaletteSO ChosePalette()
        {
            // 1. Safety check: ensure the SO and the list are valid
            if (palettesSO == null || palettesSO.palettes == null || palettesSO.palettes.Count == 0)
            {
                Debug.LogWarning("ChoseAndApply: No palettes available to choose from.");
                return null;
            }

            int count = palettesSO.palettes.Count;
            ulong rawVal = Seed.GetBaseValue();

            // 2. Determine how many digits we need based on the count
            // if count is 1-10, we need 1 digit (0-9)
            // if count is 11-100, we need 2 digits (0-99), etc.
            int digitsNeeded = 1;
            if (count > 1)
            {
                digitsNeeded = Mathf.FloorToInt(Mathf.Log10(count - 1)) + 1;
            }

            // 3. Extract the digits from the ulong
            // We use Power of 10 to create a mask (e.g., 2 digits = 100)
            long powerOf10 = (long)Mathf.Pow(10, digitsNeeded);
            ulong extractedDigits = rawVal % (ulong)powerOf10;

            // 4. Map the extracted digits to the actual list count
            // We use modulo again to ensure that if we extracted "95" 
            // but only have 12 palettes, it wraps around correctly.
            int finalIndex = (int)(extractedDigits % (ulong)count);

            return palettesSO.palettes[finalIndex];
        }
        
    }
}
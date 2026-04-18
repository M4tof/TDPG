using System.Collections;
using TDPG.VideoGeneration;
using UnityEngine;

namespace Tests.PaletteSwap
{
    public class PaletteCycler : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Time in seconds between swaps.")]
        public float cycleInterval = 2.0f;

        [Header("Palettes to Cycle")]
        public ColorPaletteSO paletteA;
        public ColorPaletteSO paletteB;

        // The interface allows us to talk to ANY of the specific controller scripts
        private IColorSwapController _controller;

        void Start()
        {
            // Try to find any component that implements our interface
            _controller = GetComponent<IColorSwapController>();

            if (_controller == null)
            {
                Debug.LogError($"[PaletteCycler] No IColorSwapController found on '{name}'! " +
                               "Please attach one of the ColorSwapController scripts.", this);
                enabled = false;
                return;
            }

            if (paletteA == null || paletteB == null)
            {
                Debug.LogWarning($"[PaletteCycler] Missing palettes on '{name}'. Please assign Palette A and B.", this);
                enabled = false;
                return;
            }

            // Start the infinite loop
            StartCoroutine(CycleRoutine());
        }

        private IEnumerator CycleRoutine()
        {
            bool isTurnA = true;

            while (true)
            {
                // 1. Pick the palette
                ColorPaletteSO paletteToUse = isTurnA ? paletteA : paletteB;

                // 2. Apply it using the interface
                // This works regardless of whether it's a Single or Multi swap controller
                _controller.SetPalette(paletteToUse);

                // 3. Flip the boolean for next time
                isTurnA = !isTurnA;

                // 4. Wait
                yield return new WaitForSeconds(cycleInterval);
            }
        }
    }
}
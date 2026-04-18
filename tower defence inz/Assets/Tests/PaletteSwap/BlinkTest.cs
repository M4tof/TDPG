using System.Collections;
using TDPG.VideoGeneration;
using UnityEngine;

namespace Tests.PaletteSwap
{
    public class BlinkTest : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Time in seconds between blinks.")]
        public float cycleInterval = 2.0f;

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

            // Start the infinite loop
            StartCoroutine(CycleRoutine());
        }

        private IEnumerator CycleRoutine()
        {

            while (true)
            {
                _controller.BlinkWhite();

                yield return new WaitForSeconds(cycleInterval);
            }
        }
    }
}
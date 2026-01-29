using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    /// <summary>
    /// Applies a 'Glitching' like effect to the audio source by rapidly toggling a pitch offset on and off.
    /// <br/>
    /// This creates a robotic stutter or signal interference sound using a high-frequency sine wave.
    /// </summary>
    [CreateAssetMenu(menuName = "TDPG/Audio/Mod/Bitwise Glitch")]
    public class BitwiseGlitch : AudioModifier
    {
        [Header("Activation Settings")]
        [Tooltip("If TRUE, this effect only activates if the specific bit in the seed is 1.\nIf FALSE, the effect is always active.")]
        public bool useBitwiseCheck = true;

        [Tooltip("Which bit (0-63) determines if the glitch happens (only used if useBitwiseCheck is true).")]
        public int triggerBitIndex = 5; 

        [Header("Effect Settings")]
        [Tooltip("How strong the modifier should 'glitch' the audio.")]
        public float glitchIntensity = 0.5f;

        private bool _active;
        // Store a random offset derived from the seed
        private float _timeOffset;

        public override void OnInitialize(AudioContext ctx)
        {
            // Determine if the effect is active
            if (useBitwiseCheck)
            {
                // Strict check: Is the specific bit set?
                _active = (ctx.SeedValue & (1UL << triggerBitIndex)) != 0;
            }
            else
            {
                // Loose check: Always active
                _active = true;
            }
            
            // Create a random offset (0.0 to 100.0) based on the seed
            _timeOffset = (float)(ctx.SeedValue % 100); 
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            if (!_active) return;

            // Add the offset to time
            float flicker = Mathf.Sin((time + _timeOffset) * 30f);
            
            if (flicker > 0)
            {
                currentPitch += glitchIntensity;
            }
        }
    }
}
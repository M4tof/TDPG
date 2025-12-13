using TDPG.Audio;
using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    [CreateAssetMenu(menuName = "TDPG/Audio/Bitwise Glitch")]
    public class BitwiseGlitch : AudioModifier
    {
        [Tooltip("If the 5th bit of the seed is 1, this effect activates.")]
        public int triggerBitIndex = 5; 
        public float glitchIntensity = 0.5f;

        private bool _active;

        public override void OnInitialize(AudioContext ctx)
        {
            // Manual bitwise check on ulong
            // 1UL shifts a 1 into the position we want.
            // & checks if that bit is present in the seed.
            _active = (ctx.SeedValue & (1UL << triggerBitIndex)) != 0;
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            if (!_active) return;

            // Fast sine wave flicker
            float flicker = Mathf.Sin(time * 30f);
            if (flicker > 0)
            {
                currentPitch += glitchIntensity;
            }
        }
    }
}
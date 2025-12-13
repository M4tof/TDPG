using TDPG.Audio;
using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    [CreateAssetMenu(menuName = "TDPG/Audio/LFO Wobble")]
    public class LFOWobble : AudioModifier
    {
        public float frequency = 1.0f; // Speed of the wobble
        public float amplitude = 0.1f; // How extreme the wobble is

        public override void OnInitialize(AudioContext ctx) { }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            // Calculate Sine wave
            float wave = Mathf.Sin(time * frequency * 2f * Mathf.PI);
        
            // Apply to pitch (1.0 +/- amplitude)
            currentPitch += (wave * amplitude);
        }
    }
}
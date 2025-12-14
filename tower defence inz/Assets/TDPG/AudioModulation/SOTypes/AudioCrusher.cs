using UnityEngine;
using TDPG.Audio;

namespace TDPG.AudioModulation.SOTypes
{
    [CreateAssetMenu(menuName = "TDPG/Audio/Audio Crusher")]
    public class AudioCrusher : AudioModifier
    {
        [Header("Distortion")]
        [Tooltip("0 = Clean, 1 = Full Crunch")]
        public Vector2 distortionRange = new Vector2(0.1f, 0.6f);
        
        [Header("Triggering")]
        [Tooltip("Only crush the audio if this specific bit is set in the seed (optional).")]
        public bool useBitwiseCheck = false;
        public int triggerBit = 3;

        public override void OnInitialize(AudioContext ctx)
        {
            // 1. Bitwise Check (Genetic Trait)
            if (useBitwiseCheck)
            {
                bool active = (ctx.SeedValue & (1UL << triggerBit)) != 0;
                if (!active) return; // Exit if gene is missing, leaving sound clean
            }

            // 2. Add Component
            AudioDistortionFilter dist = ctx.Source.GetComponent<AudioDistortionFilter>();
            if (dist == null)
            {
                dist = ctx.Source.gameObject.AddComponent<AudioDistortionFilter>();
            }

            // 3. Determine Intensity
            double r = ctx.Random.NextDouble();
            float level = Mathf.Lerp(distortionRange.x, distortionRange.y, (float)r);

            dist.distortionLevel = level;
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            // Distortion is usually static, but if you wanted "Dynamic Interference"
            // you could modify dist.distortionLevel here using Perlin noise.
        }
    }
}
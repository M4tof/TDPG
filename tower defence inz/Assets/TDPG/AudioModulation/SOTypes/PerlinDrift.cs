using TDPG.Audio;
using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    [CreateAssetMenu(menuName = "TDPG/Audio/Perlin Drift")]
    public class PerlinDrift : AudioModifier
    {
        //creates an "Organic" wandering effect.
        
        public float driftSpeed = 0.5f;
        public float intensity = 0.2f;

        public override void OnInitialize(AudioContext ctx) { }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            // Convert ulong seed to a float offset. 
            // We modulo by 99999 to prevent float precision issues with huge ulongs.
            float seedOffset = (float)(ctx.SeedValue % 99999);

            // Calculate noise
            float noise = Mathf.PerlinNoise(time * driftSpeed, seedOffset);
            
            // Map 0..1 to -1..1
            noise = (noise - 0.5f) * 2f;

            currentPitch += (noise * intensity);
        }
    }
}
using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    /// <summary>
    /// Applies a continuous, organic wandering effect to the audio pitch using Perlin Noise.
    /// <br/>
    /// Useful for making looping sounds (drones, engines, wind) feel less static without sounding random.
    /// </summary>
    [CreateAssetMenu(menuName = "TDPG/Audio/Mod/Perlin Drift")]
    public class PerlinDrift : AudioModifier
    {
        //creates an "Organic" wandering effect.
        [Tooltip("How fast the pitch wanders. Low values = slow waves, High values = fast jitter.")]
        public float driftSpeed = 0.5f;
        [Tooltip("The strength of the effect. Defines the maximum range the pitch will deviate from the original.")]
        public float intensity = 0.2f;

        public override void OnInitialize(AudioContext ctx) { }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            // Convert ulong seed to a float offset. 
            // We conduct a modulo operation by 99999 to prevent float precision issues with huge ulongs.
            float seedOffset = (float)(ctx.SeedValue % 99999);

            // Calculate noise
            float noise = Mathf.PerlinNoise(time * driftSpeed, seedOffset);
            
            // Map 0..1 to -1..1
            noise = (noise - 0.5f) * 2f;

            currentPitch += (noise * intensity);
        }
    }
}
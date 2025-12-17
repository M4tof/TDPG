using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    /// <summary>
    /// Applies a static Low Pass Filter to the audio source.
    /// <br/>
    /// This "muffles" the sound by removing high frequencies, simulating effects like being underwater, 
    /// behind a wall, or inside a helmet.
    /// </summary>
    [CreateAssetMenu(menuName = "TDPG/Audio/Spectral Muffler")]
    public class SpectralMuffler : AudioModifier
    {
        //adds an AudioLowPassFilter component to the object.
        //This muffles the sound, removing high frequencies.
        
        [Tooltip("Range of the Low Pass Cutoff. 22000 is open, 500 is very muffled. The minimum (X) and maximum (Y) cutoff frequency in Hertz.")]
        public Vector2 cutoffRange = new Vector2(500f, 22000f);

        public override void OnInitialize(AudioContext ctx)
        {
            // Try to get existing filter or add a new one
            AudioLowPassFilter filter = ctx.Source.GetComponent<AudioLowPassFilter>();
            if (filter == null)
            {
                filter = ctx.Source.gameObject.AddComponent<AudioLowPassFilter>();
            }

            // Pick a value based on seed
            double r = ctx.Random.NextDouble();
            float selectedCutoff = Mathf.Lerp(cutoffRange.x, cutoffRange.y, (float)r);

            filter.cutoffFrequency = selectedCutoff;
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            // We don't need to update every frame unless we want the muffle to drift.
            // For now, it's a static "Material Property" of the sound.
        }
    }
}
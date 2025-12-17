using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    /// <summary>
    /// Controls the stereo placement of the audio source.
    /// <br/>
    /// Can either animate the sound moving Left-to-Right (Auto-Pan) or assign a fixed random position 
    /// within the stereo field based on the seed.
    /// </summary>
    [CreateAssetMenu(menuName = "TDPG/Audio/Stereo Panner")]
    public class StereoPanner : AudioModifier
    {
        [Header("Settings")]
        [Tooltip("If true, sound moves Left<->Right over time. If false, picks a random static position.")]
        public bool autoPan = true;
        
        [Tooltip("How fast it pans (Hz). Only used if autoPan is true.")]
        public float panSpeed = 0.5f;

        [Tooltip("How far left/right it goes (0 to 1). 1 = Full Hard Pan.")]
        [Range(0f, 1f)]
        public float panWidth = 0.8f;

        private float _timeOffset;
        private float _staticPan;

        public override void OnInitialize(AudioContext ctx)
        {
            // Seed determination
            // We use the seed to pick a random starting phase for the sine wave
            // OR a random static position.
            _timeOffset = (float)(ctx.SeedValue % 100);

            // Determine static pan value (-1 to 1) based on seed
            // We map 0..1 random to -1..1 range
            float randomVal = (float)ctx.Random.NextDouble(); 
            _staticPan = Mathf.Lerp(-panWidth, panWidth, randomVal);
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            if (autoPan)
            {
                // Ping Pong logic
                float sine = Mathf.Sin((time + _timeOffset) * panSpeed * 2f * Mathf.PI);
                ctx.Source.panStereo = sine * panWidth;
            }
            else
            {
                // Static assignment (done every frame to override any other scripts fighting for it)
                ctx.Source.panStereo = _staticPan;
            }
        }
    }
}
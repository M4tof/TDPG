using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    /// <summary>
    /// Modulates the AudioSource pitch using a Low-Frequency Oscillator (LFO) sine wave.
    /// <br/>
    /// Creates a vibrato effect. Useful for unstable engines, magic hums, or sci-fi sirens.
    /// </summary>
    [CreateAssetMenu(menuName = "TDPG/Audio/Mod/LFO Wobble")]
    public class LFOWobble : AudioModifier
    {
        [Tooltip("Speed of the wobble in Hz")]
        public float frequency = 1.0f; 
        
        [Tooltip("How much the pitch changes (+/-)")]
        public float amplitude = 0.1f; 

        [Tooltip("If TRUE: Wobble cycle matches audio position (restarts when clip loops).\nIf FALSE: Wobble runs continuously on game time.")]
        public bool syncToClipLoop = false;

        private float _timeOffset;

        public override void OnInitialize(AudioContext ctx) 
        {
            _timeOffset = (float)(ctx.SeedValue % 100);
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            // Option A: 'time' is the continuous time since Play() started (Game Time).
            // Option B: 'ctx.Source.time' is the playback head position (0s -> Clip Length -> 0s).
            float timeBase = syncToClipLoop ? ctx.Source.time : time;

            // Calculate Sine wave
            float wave = Mathf.Sin((timeBase + _timeOffset) * frequency * 2f * Mathf.PI);
            
            currentPitch += (wave * amplitude);
        }
    }
}
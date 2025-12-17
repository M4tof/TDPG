using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    /// <summary>
    /// Restricts playback to a random, deterministically selected segment of the AudioClip.
    /// <br/>
    /// Useful for creating variations from a single long recording (e.g., picking a random bird chirp from a 1-minute forest ambience).
    /// </summary>
    [CreateAssetMenu(menuName = "TDPG/Audio/Clip Segmenter")]
    public class ClipSegmenter : AudioModifier
    {
        [Tooltip("Minimal allowed duration for the clip. (in seconds)")]
        public float minDuration = 0.5f;
        
        [Tooltip("Maximum allowed duration for the clip. (in seconds)")]
        public float maxDuration = 2.0f;

        private float _startTime;
        private float _endTime;
        private bool _hasClip;

        public override void OnInitialize(AudioContext ctx)
        {
            if (ctx.Source.clip == null) { _hasClip = false; return; }
            _hasClip = true;

            float totalLen = ctx.Source.clip.length;
            double randDur = ctx.Random.NextDouble();
            float duration = Mathf.Lerp(minDuration, maxDuration, (float)randDur);

            float maxStartTime = Mathf.Max(0, totalLen - duration);
            double randStart = ctx.Random.NextDouble();
            
            _startTime = Mathf.Lerp(0, maxStartTime, (float)randStart);
            _endTime = _startTime + duration;

            // FORCE position update
            ctx.Source.time = _startTime;
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            if (!_hasClip) return;
            
            // If we are essentially at 0 (start of clip) but our segment intends to start at 5.0s,
            // we force the jump.
            if (ctx.Source.time < _startTime - 0.05f) 
            {
                ctx.Source.time = _startTime;
            }

            // Standard segment loop
            if (ctx.Source.time >= _endTime)
            {
                ctx.Source.time = _startTime;
            }
        }
    }
}
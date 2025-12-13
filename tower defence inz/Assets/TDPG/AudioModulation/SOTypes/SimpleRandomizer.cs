using TDPG.Audio;
using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    [CreateAssetMenu(menuName = "TDPG/Audio/Simple Randomizer")]
    public class SimpleRandomizer : AudioModifier
    {
        public Vector2 pitchRange = new Vector2(0.9f, 1.1f);
        public Vector2 volumeRange = new Vector2(0.8f, 1.0f);

        // Temp storage for the calculated offset
        private float _pMult;
        private float _vMult;

        public override void OnInitialize(AudioContext ctx)
        {
            // We use the ctx.Random which is already seeded with your ulong
            double p = ctx.Random.NextDouble();
            double v = ctx.Random.NextDouble();

            _pMult = Mathf.Lerp(pitchRange.x, pitchRange.y, (float)p);
            _vMult = Mathf.Lerp(volumeRange.x, volumeRange.y, (float)v);
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            currentPitch *= _pMult;
            currentVolume *= _vMult;
        }
    }
}
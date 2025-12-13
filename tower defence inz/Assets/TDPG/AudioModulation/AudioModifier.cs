using TDPG.AudioModulation;
using UnityEngine;

namespace TDPG.Audio
{
    public abstract class AudioModifier : ScriptableObject
    {
        /// <summary>
        /// Called once before the sound starts.
        /// </summary>
        public abstract void OnInitialize(AudioContext ctx);

        /// <summary>
        /// Called every frame. 
        /// Use 'ctx.SeedValue' if you need deterministic noise.
        /// </summary>
        public abstract void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume);
    }
}
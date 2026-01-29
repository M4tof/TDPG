using UnityEngine;

namespace TDPG.AudioModulation.SOTypes
{
    /// <summary>
    /// Randomly assigns a physical space characteristic to the sound using Unity's <see cref="AudioReverbFilter"/>.
    /// <br/>
    /// The specific reverb preset is selected deterministically based on the seed.
    /// </summary>
    [CreateAssetMenu(menuName = "TDPG/Audio/Mod/Reverb Roulette")]
    public class ReverbRoulette : AudioModifier
    {
        [Tooltip("If TRUE, Reverb is only applied if the trigger bit is 1. If FALSE, Reverb is always applied.")]
        public bool useBitwiseCheck = true;
        [Tooltip("Only apply reverb if this bit (0-63) is set in the seed.")]
        public int triggerBit = 2; // Only apply reverb if bit 2 is set

        public override void OnInitialize(AudioContext ctx)
        {
            // 1. Bitwise Check (Optional)
            if (useBitwiseCheck)
            {
                bool active = (ctx.SeedValue & (1UL << triggerBit)) != 0;
                if (!active) return; // Exit if a gene is missing
            }

            // 2. Add Component
            AudioReverbFilter reverb = ctx.Source.GetComponent<AudioReverbFilter>();
            if (reverb == null) reverb = ctx.Source.gameObject.AddComponent<AudioReverbFilter>();

            // 3. Pick a random Preset from the Enum
            // There are roughly 10-12 presets in AudioReverbPreset
            int presetCount = System.Enum.GetNames(typeof(AudioReverbPreset)).Length;
            
            // Use seed to pick index
            int index = ctx.Random.Next(0, presetCount);
            
            // Apply
            reverb.reverbPreset = (AudioReverbPreset)index;
            
        }

        public override void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume)
        {
            // No update needed
        }
    }
}
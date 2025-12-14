using UnityEngine;
using TDPG.Audio;

namespace TDPG.AudioModulation.SOTypes
{
    [CreateAssetMenu(menuName = "TDPG/Audio/Reverb Roulette")]
    public class ReverbRoulette : AudioModifier
    {
        //It randomly assigns a physical space characteristic to the sound using Unity's AudioReverbFilter.
        
        public bool useBitwiseCheck = true;
        public int triggerBit = 2; // Only apply reverb if bit 2 is set

        public override void OnInitialize(AudioContext ctx)
        {
            // 1. Bitwise Check (Optional)
            if (useBitwiseCheck)
            {
                bool active = (ctx.SeedValue & (1UL << triggerBit)) != 0;
                if (!active) return; // Exit if gene is missing
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
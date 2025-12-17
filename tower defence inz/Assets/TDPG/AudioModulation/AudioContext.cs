using UnityEngine;

namespace TDPG.AudioModulation
{
    public class AudioContext
    {
        public AudioSource Source;
        public float BasePitch;
        public float BaseVolume;
        
        // Raw seed data
        public ulong SeedValue; 
        
        // Pre-initialized random generator for convenience
        public System.Random Random; 

        public AudioContext(AudioSource source, ulong seedValue)
        {
            Source = source;
            BasePitch = source.pitch;
            BaseVolume = source.volume;
            SeedValue = seedValue;

            // Initialize Random using the ulong. 
            // We use 'unchecked' to safely cast ulong -> int without overflow errors.
            int seedAsInt = unchecked((int)seedValue);
            Random = new System.Random(seedAsInt);
        }
    }
}
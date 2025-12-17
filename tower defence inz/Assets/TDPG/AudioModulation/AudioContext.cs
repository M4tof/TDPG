using UnityEngine;

namespace TDPG.AudioModulation
{
    /// <summary>
    /// A data container that holds the state of an active audio instance.
    /// Passed to <see cref="AudioModifier"/> methods to ensure deterministic behavior.
    /// </summary>
    public class AudioContext
    {
        /// <summary>
        /// The Unity <see cref="AudioSource"/> that is being modified.
        /// </summary>
        public AudioSource Source;

        /// <summary>
        /// The original pitch of the AudioSource before any modifiers were applied.
        /// </summary>
        public float BasePitch;

        /// <summary>
        /// The original volume of the AudioSource before any modifiers were applied.
        /// </summary>
        public float BaseVolume;
        
        /// <summary>
        /// The raw seed value used to generate this context. 
        /// Useful for bitwise checks or generating unique deterministic modifications.
        /// Should but doesn't need to come from <see cref="TDPG.Generators.Seed.Seed"/>
        /// </summary>
        public ulong SeedValue; 
        
        /// <summary>
        /// A pre-initialized random number generator based on the <see cref="SeedValue"/>.
        /// <br/>
        /// <b>Important:</b> Always use this instead of <see cref="UnityEngine.Random"/> 
        /// to ensure the audio behaves identically every time the same Seed is used.
        /// </summary>
        public System.Random Random; 

        /// <summary>
        /// Creates a new context and initializes the Random provider.
        /// </summary>
        /// <param name="source">The target AudioSource.</param>
        /// <param name="seedValue">A unique seed (e.g., from an entity ID or global RNG like <see cref="TDPG.Generators.Seed.GlobalSeed"/>).</param>
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
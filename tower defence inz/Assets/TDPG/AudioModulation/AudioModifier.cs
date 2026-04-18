using UnityEngine;

namespace TDPG.AudioModulation
{
    /// <summary>
    /// Abstract base class from which all TPDG Audio Scriptable Objects must inherit.
    /// <br/>
    /// Create concrete implementations (like <see cref="TDPG.AudioModulation.SOTypes.AudioCrusher"/>) 
    /// to define custom audio behaviors.
    /// </summary>
    public abstract class AudioModifier : ScriptableObject
    {
        /// <summary>
        /// Called once when the audio instance is created or before the sound starts playing.
        /// Use this to add components (filters) or set initial values.
        /// </summary>
        /// <param name="ctx">The runtime context containing the Source, Random provider, and Seed.</param>
        public abstract void OnInitialize(AudioContext ctx);

        /// <summary>
        /// Called every frame while the audio is playing.
        /// Allows for continuous modification of pitch and volume over time.
        /// </summary>
        /// <param name="ctx">The runtime context of the modified source.</param>
        /// <param name="time">The elapsed time (in seconds) since the modifier started.</param>
        /// <param name="currentPitch">
        /// A reference to the current pitch. Modify this value to apply pitch bending or vibrato. 
        /// <br/>(Multiply for relative changes, Add for absolute changes).
        /// </param>
        /// <param name="currentVolume">
        /// A reference to the current volume (0.0 to 1.0). Modify this value to apply tremolo or fades.
        /// </param>
        public abstract void OnUpdate(AudioContext ctx, float time, ref float currentPitch, ref float currentVolume);
    }
}
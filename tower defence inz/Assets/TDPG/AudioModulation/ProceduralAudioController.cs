using System.Collections.Generic;
using TDPG.Generators.Seed;
using UnityEngine;

namespace TDPG.AudioModulation
{
    /// <summary>
    /// Controller used by the TDPG library to provide procedural audio modification.
    /// <br/>
    /// It must be placed on the same GameObject as an <see cref="UnityEngine.AudioSource"/>.
    /// It orchestrates a list of <see cref="AudioModifier"/>s to alter sound deterministically based on a seed.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ProceduralAudioController : MonoBehaviour
    {
        /// <summary>
        /// The raw seed value used by the modifiers.
        /// <br/>
        /// Useful for bitwise checks or generating unique deterministic modifications.
        /// <para>
        /// This can be set manually, or populated via <see cref="Play(ulong)"/>. 
        /// Default is "123456789".
        /// </para>
        /// </summary>
        [Header("Configuration")]
        [Tooltip("The seed used for deterministic generation. Can be set manually or via code.")]
        public ulong seedValue = 123456789;
        
        /// <summary>
        /// A list of <see cref="AudioModifier"/> ScriptableObjects to apply to the source.
        /// <br/>
        /// The library executes them in sequential order. 
        /// Common examples include <see cref="TDPG.AudioModulation.SOTypes.AudioCrusher"/>.
        /// </summary>
        [Tooltip("List of modifiers to apply. Order matters (sequential execution).")]
        public List<AudioModifier> modifiers = new List<AudioModifier>();
        
        /// <summary>
        /// Sets whether the sound should play immediately in the <see cref="Start"/> method.
        /// <para>
        /// <b>Warning:</b> This needs to be used instead of the standard "Play On Awake" flag of the AudioSource to ensure
        /// modifiers are initialized before playback begins.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Not recommended for complex synchronization as modifications might cause a tiny delay (DSP scheduling).
        /// </remarks>
        [Header("Debug")]
        [Tooltip("If true, triggers Play() automatically in Start().")]
        [SerializeField] private bool playOnStart = true;

        private AudioSource _source;
        private AudioContext _context;
        private bool _isPlaying;
        private double _dspStartTime;
        
        private float _inspectorPitch;
        private float _inspectorVolume;

        void Awake()
        {
            _source = GetComponent<AudioSource>();
            
            // We disable the default behavior so we can inject our logic first
            _source.playOnAwake = false;
            
            _inspectorPitch = _source.pitch;
            _inspectorVolume = _source.volume;
        }

        void Start()
        {
            if (playOnStart) Play();
        }

        /// <summary>
        /// Convenience overload to play audio using a <see cref="Seed"/> object.
        /// </summary>
        /// <param name="seedObj">The seed object. If null, falls back to a default value.</param>
        public void Play(Seed seedObj) => Play(seedObj != null ? seedObj.GetBaseValue() : 12345);

        /// <summary>
        /// Updates the seed value and starts playback.
        /// </summary>
        /// <param name="rawSeed">The new raw seed value to use for this playback instance.</param>
        public void Play(ulong rawSeed)
        {
            this.seedValue = rawSeed;
            Play();
        }

        /// <summary>
        /// Initializes the context, applies <see cref="AudioModifier.OnInitialize"/> logic, 
        /// and schedules the audio to play with a slight DSP delay.
        /// </summary>
        public void Play()
        {
            // Reset to inspector defaults before applying new modifiers
            _source.pitch = _inspectorPitch;
            _source.volume = _inspectorVolume;

            _context = new AudioContext(_source, seedValue);

            foreach (var mod in modifiers)
            {
                if (mod != null) mod.OnInitialize(_context);
            }

            // Schedule slightly in the future to ensure audio thread sync
            _dspStartTime = AudioSettings.dspTime + 0.1;
            
            // Apply initial frame 0 modulation
            ApplyModulation(0f);

            _source.PlayScheduled(_dspStartTime);
            _isPlaying = true;
        }

        void Update()
        {
            if (!_isPlaying || !_source.isPlaying) return;
            
            // Calculate precise time relative to the scheduled start
            double timeAlive = AudioSettings.dspTime - _dspStartTime;
            
            float safeTime = (float)System.Math.Max(0, timeAlive);
            
            ApplyModulation(safeTime);
        }

        /// <summary>
        /// Iterates through modifiers to calculate and apply the current frame's pitch and volume.
        /// </summary>
        /// <param name="time">Time elapsed since playback started.</param>
        private void ApplyModulation(float time)
        {
            float proposedPitch = _context.BasePitch;
            float proposedVolume = _context.BaseVolume;

            foreach (var mod in modifiers)
            {
                // Pass by Ref allows modifiers to stack their effects
                if (mod != null) mod.OnUpdate(_context, time, ref proposedPitch, ref proposedVolume);
            }

            _source.pitch = proposedPitch;
            _source.volume = proposedVolume;
        }
    }
}
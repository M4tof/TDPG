using System.Collections.Generic;
using TDPG.Audio;
using TDPG.Generators.Seed;
using UnityEngine;

namespace TDPG.AudioModulation
{
    [RequireComponent(typeof(AudioSource))]
    public class ProceduralAudioController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The raw seed value. Can be set via Inspector for testing.")]
        public ulong seedValue = 123456789;

        [Tooltip("Drag your SO building blocks here")]
        public List<AudioModifier> modifiers = new List<AudioModifier>();

        [Header("Debug")]
        [SerializeField] private bool playOnStart = true;

        private AudioSource _source;
        private AudioContext _context;
        private bool _isPlaying;

        void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (playOnStart) Play();
        }

        // --- Overload 1: Use Seed Class ---
        public void Play(Seed seedObj)
        {
            if (seedObj != null)
                Play(seedObj.GetBaseValue());
            else
                Play(12345); // Fallback
        }

        // --- Overload 2: Use Raw ulong ---
        public void Play(ulong rawSeed)
        {
            this.seedValue = rawSeed;
            Play();
        }

        // --- Main Play Logic ---
        public void Play()
        {
            // 1. Create Context
            _context = new AudioContext(_source, seedValue);

            // 2. Initialize Modifiers
            foreach (var mod in modifiers)
            {
                if (mod != null) mod.OnInitialize(_context);
            }

            // 3. Start Audio
            _source.Play();
            _isPlaying = true;
        }

        void Update()
        {
            if (!_isPlaying || !_source.isPlaying) return;

            // Always start calculation from the base settings
            float proposedPitch = _context.BasePitch;
            float proposedVolume = _context.BaseVolume;
            float time = Time.time;

            // Apply all modifiers
            foreach (var mod in modifiers)
            {
                if (mod != null) mod.OnUpdate(_context, time, ref proposedPitch, ref proposedVolume);
            }

            _source.pitch = proposedPitch;
            _source.volume = proposedVolume;
        }
    }
}
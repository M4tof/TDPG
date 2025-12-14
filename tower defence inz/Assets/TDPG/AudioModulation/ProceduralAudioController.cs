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
        public ulong seedValue = 123456789;
        public List<AudioModifier> modifiers = new List<AudioModifier>();

        [Header("Debug")]
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
            _source.playOnAwake = false;
            
            _inspectorPitch = _source.pitch;
            _inspectorVolume = _source.volume;
        }

        void Start()
        {
            if (playOnStart) Play();
        }

        public void Play(Seed seedObj) => Play(seedObj != null ? seedObj.GetBaseValue() : 12345);
        public void Play(ulong rawSeed)
        {
            this.seedValue = rawSeed;
            Play();
        }

        public void Play()
        {
            _source.pitch = _inspectorPitch;
            _source.volume = _inspectorVolume;

            _context = new AudioContext(_source, seedValue);

            foreach (var mod in modifiers)
            {
                if (mod != null) mod.OnInitialize(_context);
            }

            _dspStartTime = AudioSettings.dspTime + 0.1;
            
            ApplyModulation(0f);

            _source.PlayScheduled(_dspStartTime);
            _isPlaying = true;
        }

        void Update()
        {
            if (!_isPlaying || !_source.isPlaying) return;
            
            double timeAlive = AudioSettings.dspTime - _dspStartTime;
            
            float safeTime = (float)System.Math.Max(0, timeAlive);
            
            ApplyModulation(safeTime);
        }

        private void ApplyModulation(float time)
        {
            float proposedPitch = _context.BasePitch;
            float proposedVolume = _context.BaseVolume;

            foreach (var mod in modifiers)
            {
                if (mod != null) mod.OnUpdate(_context, time, ref proposedPitch, ref proposedVolume);
            }

            _source.pitch = proposedPitch;
            _source.volume = proposedVolume;
        }
    }
}
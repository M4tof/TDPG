using System.Collections.Generic;
using TDPG.Generators.Seed;
using UnityEditor;
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
        /// Should the controller pick the modifications manually from the given modifiers or based on lists (<see cref="AllowedModsListSO"/>, <see cref="AudioListSO"/>).
        /// </summary>
        public enum GenerationMode
        {
            /// <summary>
            /// Use <see cref="ProceduralAudioController.modifiers"/> and the currently assigned clip on the AudioSource.
            /// </summary>
            Manual, 
            
            /// <summary>
            /// Use <see cref="ProceduralAudioController.clipPool"/> and <see cref="ProceduralAudioController.modifierPool"/> to pick content.
            /// </summary>
            Procedural
        }
        
        [Header("Mode Selection")]
        [Tooltip("Manual: Use the specific clip and modifiers assigned below.\nProcedural: Pick from the SO Lists using the Selection Seed.")]
        public GenerationMode mode = GenerationMode.Manual;
        
        [Header("General Seeds")]
        [Tooltip("Seed A: Used for the 'Modulation' (the internal logic of modifiers).")]
        public ulong modulationSeed = 123456789;

        [Tooltip("Seed B: Used for 'Selection' (picking which clip and which modifiers to use).")]
        public ulong selectionSeed = 987654321;
        
        [Header("Procedural Sources (Mode: Procedural)")]
        public AudioListSO clipPool;
        public AllowedModsListSO modifierPool;
        [Range(0, 10)] public int maxModifiersToPick = 2;
        
        /// <summary>
        /// A list of <see cref="AudioModifier"/> ScriptableObjects to apply to the source.
        /// <br/>
        /// The library executes them in sequential order. 
        /// Common examples include <see cref="TDPG.AudioModulation.SOTypes.AudioCrusher"/>.
        /// </summary>
        [Header("Active Configuration")]
        [Tooltip("The list of modifiers currently active. In Manual mode, you set these. In Procedural mode, these are generated.")]
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

        /// <summary>
        /// Triggers the selection logic (if in Procedural mode) and starts playback.
        /// </summary>
        [ContextMenu("Generate and Play")]
        public void GenerateAndPlay()
        {
            Generate();
            Play();
        }

        public void Generate()
        {
            EnsureDependencies();

            if (mode == GenerationMode.Procedural)
            {
                ApplyProceduralSelection();
            }

            // In Editor Mode (not playing), we can't use PlayScheduled, so we just assign the data.
            if (!Application.isPlaying)
            {
                Debug.Log("[ProceduralAudioController] Generated configuration in Editor. Press Play to hear it.");
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(_source);
#endif
                return;
            }
        }
        
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
            // Call GenerateAndPlay instead of Play to ensure procedural logic runs
            if (playOnStart) GenerateAndPlay();
        }

        /// <summary>
        /// Ensures the AudioSource reference is valid, even if called from the Editor.
        /// </summary>
        private void EnsureDependencies()
        {
            if (_source == null) _source = GetComponent<AudioSource>();
        }
        
        /// <summary>
        /// Uses the Selection Seed to pick a clip and modifiers from the provided SO lists.
        /// </summary>
        public void ApplyProceduralSelection()
        {
            if (clipPool == null || modifierPool == null)
            {
                Debug.LogWarning($"[ProceduralAudioController] Procedural mode active on {gameObject.name} but Lists are missing!");
                return;
            }

            // We use a simple C# Random initialized with the selectionSeed for picking
            // This ensures that if the seed is the same, the selection is the same.
            System.Random prng = new System.Random((int)selectionSeed);

            // 1. Select Clip
            if (clipPool.audioClips.Count > 0)
            {
                int clipIndex = prng.Next(clipPool.audioClips.Count);
                _source.clip = clipPool.audioClips[clipIndex];
                Debug.Log($"[ProceduralAudioController] source has been set to {clipPool.audioClips[clipIndex].name}");
            }
            else
            {
                Debug.Log("clipPool is empty");
            }

            // 2. Select Modifiers
            modifiers.Clear();
            if (modifierPool.allowedMods.Count > 0)
            {
                int countToPick = prng.Next(0, maxModifiersToPick + 1);
                for (int i = 0; i < countToPick; i++)
                {
                    int modIndex = prng.Next(modifierPool.allowedMods.Count);
                    modifiers.Add(modifierPool.allowedMods[modIndex]);
                    Debug.Log($"[ProceduralAudioController] Mods have been set to {modifierPool.allowedMods[modIndex].name}");
                }
            }
        }

        /// <summary>
        /// Convenience overload to play audio using a <see cref="Seed"/> object.
        /// </summary>
        /// <param name="seedObj">The seed object. If null, falls back to a default value.</param>
        public void Play(Seed seedObj) => Play(seedObj != null ? seedObj.GetBaseValue() : 12345, selectionSeed);

        /// <summary>
        /// Updates both seed values and starts the generation/playback process.
        /// </summary>
        /// <param name="modSeed">The new raw seed value for modifier logic.</param>
        /// <param name="selSeed">The new raw seed value for procedural selection.</param>
        public void Play(ulong modSeed, ulong selSeed)
        {
            this.modulationSeed = modSeed;
            this.selectionSeed = selSeed;
            GenerateAndPlay();
        }

        /// <summary>
        /// Initializes the context, applies <see cref="AudioModifier.OnInitialize"/> logic, 
        /// and schedules the audio to play with a slight DSP delay.
        /// </summary>
        public void Play()
        {
            if (_source.clip == null)
            {
                Debug.LogWarning($"[ProceduralAudioController] Play called on {gameObject.name} but no AudioClip is assigned!");
                Generate();
            }

            // Reset to inspector defaults before applying new modifiers
            _source.pitch = _inspectorPitch;
            _source.volume = _inspectorVolume;

            // Use the Modulation Seed for the context
            _context = new AudioContext(_source, modulationSeed);

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
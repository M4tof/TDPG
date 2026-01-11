using System;
using System.Collections.Generic;
using System.Linq;
using TDPG.Generators.Seed;
using UnityEngine;
using Newtonsoft.Json;

namespace TDPG.EffectSystem.ElementLogic
{ 
    /// <summary>
    /// Represents a composite gameplay element (e.g., "Fire", "Toxic Sludge", "Holy Light").
    /// <br/>
    /// An Element acts as a container for multiple <see cref="Effect"/> instances. 
    /// It is generated procedurally by interpreting a <see cref="Seed"/> as a bitmask: 
    /// if Bit X is set, the Effect associated with Index X is added.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// The display name of this element (e.g., "Frost").
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// The unique numerical identifier for this element instance.
        /// </summary>
        public int Id { get; internal set; }
        
        /// <summary>
        /// The list of active gameplay effects derived from the Seed.
        /// </summary>
        public List<Effect> effects = new();
        
        /// <summary>
        /// Additional debug or lore information (e.g., the string representation of the seed).
        /// </summary>
        public List<string> MetaData { get; internal set; } = new();
        
        private readonly Seed dna;
        private float[] values;
        
        /// <summary>
        /// Helper utility to ensure the input float array is large enough for a specific effect's requirements.
        /// <br/>
        /// If the input is too short, it repeats the existing pattern to fill the gap.
        /// </summary>
        private static float[] NormalizeValues(float[] input, int needed)
        {
            if (input == null || input.Length == 0)
                return Enumerable.Repeat(0.1f, needed).ToArray();

            if (input.Length >= needed)
                return input;

            // repeat pattern until we reach needed count
            List<float> extended = new List<float>(input);
            while (extended.Count < needed)
                extended.AddRange(input);
            return extended.Take(needed).ToArray();
        }
        
        // --- Effect factory registry ---
        /// <summary>
        /// Registry mapping specific bit indices (0-63) to factory functions that instantiate concrete <see cref="Effect"/>s.
        /// <br/>
        /// <b>Key:</b> Bit Index in the Seed.
        /// <br/>
        /// <b>Value:</b> Function that takes a float array and returns an Effect.
        /// </summary>
        public static readonly Dictionary<int, Func<float[], Effect>> EffectFactories = new()
        {
            {
                0, values => {
                    var v = NormalizeValues(values, 1);
                    return new SlowDown(v[0]);
                }
            },
            {
                1, values => {
                    var v = NormalizeValues(values, 1);
                    return new HealthDown(v[0]);
                }
            },
            {
                2, values => {
                    var v = NormalizeValues(values, 1);
                    return new Heal(v[0]);
                }
            },
            {
                3, values => {
                    var v = NormalizeValues(values, 2);
                    return new TempSlowDown(v[0], v[1]);
                }
            },
            {
                4, values => {
                    var v = NormalizeValues(values, 3);
                    return new HealthDrain(v[0], v[1], v[2]);
                }
            },
            {
                5, values => {
                    var v = NormalizeValues(values, 1);
                    return new Stun(v[0]);
                }
            },
            {
                6, values => {
                    var v = NormalizeValues(values, 1);
                    return new Scale(v[0]);
                }
            }
            //TODO: LONG TERM FILL HERE
        };
        
        /// <summary>
        /// <b>Procedural Constructor:</b> Creates an Element by decoding a Seed.
        /// </summary>
        /// <remarks>
        /// Iterates through the 64 bits of the <paramref name="dna"/>. 
        /// If bit <i>i</i> is 1, the effect registered at index <i>i</i> in <see cref="EffectFactories"/> is instantiated.
        /// </remarks>
        /// <param name="name">Name of the element.</param>
        /// <param name="id">Unique ID.</param>
        /// <param name="dna">The seed. Bits determine which effects are active.</param>
        /// <param name="values">
        /// Shared pool of numerical values used to configure the intensity/duration of the created effects.
        /// </param>
        public Element(string name, int id, Seed dna, params float[] values)
        {
            Name = name;
            Id = id;
            this.dna = dna;
            this.values = values ?? Array.Empty<float>();

            dna.NormalizeSeedValue();

            for (int i = 0; i < 64; i++)
            {
                if (dna.IsBitSet(i) && EffectFactories.TryGetValue(i, out var factory))
                    effects.Add(factory(this.values));
            }

            MetaData = new List<string> { dna.ToString() };
        }
        
        //TODO: LONG TERM FILL HERE
        // --- Reverse lookup map (bit -> effect type) ---
        /// <summary>
        /// Reverse registry mapping Index -> Type. Used when reconstructing a Seed from a list of effects.
        /// </summary>
        public static readonly Dictionary<int, Type> EffectTypes = new()
        {
            { 0, typeof(SlowDown) },
            { 1, typeof(HealthDown) },
            { 2, typeof(Heal) },
            { 3, typeof(TempSlowDown) },
            { 4, typeof(HealthDrain) },
            { 5, typeof(Stun) },
            { 6, typeof(Scale) }
        };
        
        /// <summary>
        /// <b>Manual Constructor:</b> Creates an Element from an explicit list of effects.
        /// </summary>
        /// <remarks>
        /// This constructor "reverse engineers" the Seed. It checks the provided list of effects against 
        /// <see cref="EffectTypes"/> to construct the correct bitmask for the <see cref="dna"/>.
        /// This constructor should be used by the developer to make the hard-coded elements in his game.
        /// </remarks>
        /// <param name="name">Name of the element.</param>
        /// <param name="id">Unique ID.</param>
        /// <param name="effects">The list of effects to include.</param>
        public Element(string name, int id, List<Effect> effects)
        {
            Name = name;
            Id = id;
            this.effects.AddRange(effects ?? new());

            ulong seedValue = 0;
            foreach (var effect in this.effects)
            {
                var match = EffectTypes.FirstOrDefault(kvp => kvp.Value == effect.GetType());
                if (!match.Equals(default(KeyValuePair<int, Type>)))
                    seedValue |= (1UL << match.Key);
                else
                    Debug.LogWarning($"[Element] No bit mapping for {effect.GetType().Name}, treats it as 0");
            }

            dna = new Seed(seedValue, -1, "SeedFromEffectList");
            MetaData = new List<string> { dna.ToString() };
        }

        /// <summary>
        /// Constructor used only for JSON deserialization. 
        /// <br/>
        /// Assumes <paramref name="dna"/> and <paramref name="effects"/> have already been matched correctly by the serializer.
        /// </summary>
        internal Element(string name, int id, Seed dna, List<Effect> effects)
        {
            Debug.Log($"[Element] Deserialization constructor called for {name}");
            Name = name;
            Id = id;
            this.dna = dna;
            this.effects = effects ?? new List<Effect>();
            MetaData = new List<string> { dna.ToString() };
        }

        /// <summary>
        /// Appends a string to the debug metadata list.
        /// </summary>
        public void AddMetaData(string metaData) => MetaData.Add(metaData);

        /// <summary>
        /// Retrieves the list of active effects.
        /// </summary>
        public List<Effect> GetEffects() => effects;
        
        /// <summary>
        /// Retrieves the seed responsible for this element's composition.
        /// </summary>
        public Seed GetDna() => dna;

        public override string ToString()
        {
            return $"Name: {Name}, Id: {Id},  MetaData: {string.Join(",", MetaData)}, Seed: {dna}";
        }

        private static JsonSerializerSettings DefaultSettings => new()
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new EffectConverter(),
                new ElementConverter(),
                new SeedConverter()
            }
        };

        /// <summary>
        /// Serializes this Element to a JSON string using custom converters.
        /// </summary>
        public string Serialize() => JsonConvert.SerializeObject(this, DefaultSettings);
        
        /// <summary>
        /// Reconstructs an Element from a JSON string using custom converters.
        /// </summary>
        public static Element Deserialize(string json) => JsonConvert.DeserializeObject<Element>(json, DefaultSettings);

        /// <summary>
        /// Renames the element and returns the previous name.
        /// </summary>
        public string ReNameElement(string newName)
        {
            string oldName = Name;
            this.Name = newName;
            return oldName;
        }
        
    }
}
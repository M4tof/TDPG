using System.Collections;
using TDPG.AudioModulation;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.AudioTest
{
    /// <summary>
    /// Test wrapper to simulate enemy spawning and trigger procedural audio logic.
    /// </summary>
    public class EnemySpawnSimulator : MonoBehaviour
    {
        [Header("Target")]
        public ProceduralAudioController audioController;

        [Header("Seeds Configuration")]
        [Tooltip("The seed used to pick the Clip and Modifiers from the pools.")]
        public ulong selectionSeed = 987654321;
        
        [Tooltip("The seed used to drive the internal logic of the modifiers (pitch shifting, etc).")]
        public ulong modulationSeed = 123456789;

        [Header("Randomization Settings")]
        [Tooltip("If true, picks a new Selection Seed every trigger (changes the clip/mod list).")]
        public bool randomizeSelection = true;
        
        [Tooltip("If true, picks a new Modulation Seed every trigger (changes the 'flavor' of the modifiers).")]
        public bool randomizeModulation = false;

        [Header("Testing Mode")]
        [Tooltip("Should we test using the raw ulongs or the Seed object wrapper?")]
        public bool useSeedObjectOverload = false;

        [Header("Auto-Loop")]
        public bool loopTest = false;
        public float loopInterval = 3.0f;
        public float simulatedSpawnDelay = 1.0f;

        private IEnumerator Start()
        {
            Debug.Log("<color=yellow>[Simulator]</color> Initializing test environment...");
            yield return new WaitForSeconds(simulatedSpawnDelay);

            TriggerSound();

            if (loopTest)
            {
                while (true)
                {
                    yield return new WaitForSeconds(loopInterval);
                    TriggerSound();
                }
            }
        }

        /// <summary>
        /// Triggers the audio controller using the new seed logic.
        /// </summary>
        [ContextMenu("Trigger Audio Now")]
        public void TriggerSound()
        {
            if (audioController == null)
            {
                Debug.LogError("[Simulator] No Audio Controller assigned!");
                return;
            }

            // 1. Handle Randomization
            if (randomizeSelection) selectionSeed = (ulong)Random.Range(0, 999999);
            if (randomizeModulation) modulationSeed = (ulong)Random.Range(0, 999999);

            Debug.Log($"<color=green>[Simulator]</color> Triggering Sound | SelSeed: {selectionSeed} | ModSeed: {modulationSeed}");

            // 2. Test the different Play overloads
            if (useSeedObjectOverload)
            {
                // Create the seed object (it will use its base value for modulation, 
                // and fall back to the controller's current selection seed).
                Seed mockSeed = new Seed(modulationSeed, 1, "MockEnemyModulation");
                audioController.Play(mockSeed);
            }
            else
            {
                // Directly test the new dual-seed logic
                audioController.Play(modulationSeed, selectionSeed);
            }
        }
    }
}
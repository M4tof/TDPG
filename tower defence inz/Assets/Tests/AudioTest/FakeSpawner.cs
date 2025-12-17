using System.Collections;
using TDPG.AudioModulation;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.AudioTest
{
    public class EnemySpawnSimulator : MonoBehaviour
    {
        [Header("Target")]
        public ProceduralAudioController audioController;

        [Header("Settings")]
        [Tooltip("How long to wait before playing the sound (simulating spawn animation)")]
        public float simulatedSpawnDelay = 2.0f;
    
        [Tooltip("If true, it will pick a new random seed every time it plays")]
        public bool randomizeSeed = true;

        [Header("Auto-Loop")]
        [Tooltip("Keep re-playing to test stability?")]
        public bool loopTest = false;
        public float loopInterval = 3.0f;

        private IEnumerator Start()
        {
            // 1. Initial wait (Simulating the enemy loading in)
            Debug.Log($"<color=yellow>[Simulator]</color> Enemy instantiated. Waiting {simulatedSpawnDelay}s for initialization...");
            yield return new WaitForSeconds(simulatedSpawnDelay);

            // 2. Trigger the sound
            TriggerSound();

            // 3. Optional Looping to test many variations quickly
            if (loopTest)
            {
                while (true)
                {
                    yield return new WaitForSeconds(loopInterval);
                    TriggerSound();
                }
            }
        }

        // You can also click the "Context Menu" (three dots) on this script component to trigger this manually
        [ContextMenu("Trigger Audio Now")]
        public void TriggerSound()
        {
            if (audioController == null)
            {
                Debug.LogError("No Audio Controller assigned!");
                return;
            }

            // Generate a mock seed (like your generator would)
            ulong seedValue = randomizeSeed ? (ulong)Random.Range(0, 9999999) : 11111111UL;
        
            // Use your Seed class
            Seed mockEnemySeed = new Seed(seedValue, 1, "MockEnemy");

            Debug.Log($"<color=green>[Simulator]</color> Enemy Ready. Playing Sound with Seed: <b>{seedValue}</b>");
        
            // This calls the logic we wrote (Initialize -> Schedule -> Play)
            audioController.Play(mockEnemySeed);
        }
    }
}
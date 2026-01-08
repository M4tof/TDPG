using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.VideoGeneration;
using TDPG.AudioModulation;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.EventSystems.EventTrigger;

namespace TDPG.Templates.Enemies
{
    /// <summary>
    /// The Unity MonoBehaviour wrapper acting as the Controller/View for an enemy entity.
    /// <br/>
    /// It manages the link between the Unity Scene (Transform, Coroutines, Collision) and the 
    /// pure logic model (<see cref="EnemyBase"/>).
    /// </summary>
    [RequireComponent(typeof(BaseColorSwapController))]
    public class EnemyBaseBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The pure data model containing stats and state (HP, Speed).
        /// </summary>
        public EnemyBase Logic { get; private set; }

        // Registry of active status effect coroutines (Key = Effect ID).
        // Used to handle overwriting/refreshing durations.
        private Dictionary<string, Coroutine> effectCoroutines = new Dictionary<string, Coroutine>();

        /// <summary>
        /// Links this behavior to a specific logic instance and synchronizes initial state.
        /// </summary>
        /// <param name="logic">The data model instance.</param>

        [Tooltip("Color palette used for this enemy.")]
        [SerializeField] private ColorPaletteSO colorPalette;

        [SerializeField] private AudioResource screamSound;

        private BaseColorSwapController colorSwapController;
        private AudioSource audioSource;
        private ProceduralAudioController audioController;

        private void Start()
        {
            colorSwapController = gameObject.GetComponent<BaseColorSwapController>();
            audioSource = GetComponent<AudioSource>();
            audioController = GetComponent<ProceduralAudioController>();
            
            if (audioSource != null)
            {
                audioSource.resource = screamSound;
            }
            
        }
        
        public void Initialize(EnemyBase logic)
        {
            Logic = logic;
            transform.position = Logic.Position;
            Logic.OnCreation();
        }
        
        /// <summary>
        /// Handles the destruction sequence of the enemy.
        /// <br/>
        /// Triggers logical death events and destroys/pools the GameObject.
        /// </summary>
        public virtual void Die()
        {
            //EnemyCompendium.Instance.UnregisterEnemy(Logic);
            Logic.OnDeath();
            Destroy(gameObject); // TODO: Replace with Object Pooling
        }

        /// <summary>
        /// Applies damage to the internal logic model and checks for death conditions.
        /// </summary>
        /// <param name="damage">Amount of health to remove.</param>
        public virtual void DealDamage(int damage)
        {
            //Debug.Log($"DEAL {damage} DMG");
            Logic.DealDamage(damage);
            colorSwapController.BlinkWhite();
            audioController.Play();
            
            //Debug.Log($"HP {Logic.GetCurrentHealth()}");
            if (Logic.GetCurrentHealth() <= 0)
            {
                Die();
            }
        }


        /*public void Attack()
        {
            Debug.Log($"ENEMY ATTACK ({Logic.GetCurrentDamage()})");
        }*/
        

        /// <summary>
        /// Updates the movement speed in the logic model.
        /// <br/>
        /// Virtual to allow overrides for animation speed updates.
        /// </summary>
        public virtual void SetCurrentSpeed(float speed)
        {
            Logic.SetCurrentSpeed(speed);
        }

        /// <summary>
        /// A utility coroutine that pauses execution for a set time.
        /// </summary>
        public IEnumerator ApplyWait(float duration)
        {
            yield return new WaitForSeconds(duration);
            // Kod po oczekiwaniu
        }

        /// <summary>
        /// Schedules a delayed action (e.g., removing a debuff). 
        /// <br/>
        /// <b>Refresh Behavior:</b> If an effect with the same <paramref name="effectId"/> is already running, 
        /// it is stopped (cancelled) and restarted with the new duration.
        /// </summary>
        /// <param name="effectId">Unique key for the effect (e.g. "slow_debuff").</param>
        /// <param name="action">The callback to execute when time expires.</param>
        /// <param name="duration">Time in seconds to wait.</param>
        public void ApplyOrExtendEffect(string effectId, Action action, float duration)
        {
            // Jeśli efekt już istnieje, zatrzymaj go
            if (effectCoroutines.ContainsKey(effectId) && effectCoroutines[effectId] != null)
            {
                StopCoroutine(effectCoroutines[effectId]);
            }
    
            // Uruchom nową korutynę z łącznym czasem
            effectCoroutines[effectId] = StartCoroutine(EffectRoutine(effectId, action, duration));
        }

        private IEnumerator EffectRoutine(string effectId, Action action, float duration)
        {
            yield return new WaitForSeconds(duration);
            action?.Invoke();
            effectCoroutines.Remove(effectId);
        }
        
        /// <summary>
        /// Schedules a repeating action (e.g., Damage over Time).
        /// <br/>
        /// <b>Refresh Behavior:</b> If this ID exists, the previous sequence is cancelled and a new one starts from iteration 0.
        /// </summary>
        /// <param name="effectId">Unique key for the effect (e.g. "poison_dot").</param>
        /// <param name="action">Callback executed per tick. Receives the iteration index (0 to N-1).</param>
        /// <param name="iterations">How many times the action should fire.</param>
        /// <param name="interval">Seconds between ticks.</param>
        public void ApplyOrExtendIterableEffect(string effectId, Action<int> action, int iterations, float interval)
        {
            // Jeśli efekt już istnieje, zatrzymaj go
            if (effectCoroutines.ContainsKey(effectId) && effectCoroutines[effectId] != null)
            {
                StopCoroutine(effectCoroutines[effectId]);
            }

            // Uruchom nową korutynę
            effectCoroutines[effectId] = StartCoroutine(EffectRoutine(effectId, action, iterations, interval));
        }

        private IEnumerator EffectRoutine(string effectId, Action<int> action, int iterations, float interval)
        {
            for (int i = 0; i < iterations; i++)
            {
                yield return new WaitForSeconds(interval);
                action?.Invoke(i); // Przekazujemy numer iteracji (0-based)
            }
    
            effectCoroutines.Remove(effectId);
        }

        
        
        /*public void ApplyTempEffect(Action action, float duration)
        {
            StartCoroutine(DelayedActionRoutine(action, duration));
        }

        private IEnumerator DelayedActionRoutine(Action action, float duration)
        {
            yield return new WaitForSeconds(duration);
            action?.Invoke();
        }*/
    }
}

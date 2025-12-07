using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace TDPG.Templates.Enemies
{
    public class EnemyBaseBehaviour : MonoBehaviour
    {
        public EnemyBase Logic { get; private set; }
        
        private Dictionary<string, Coroutine> effectCoroutines = new Dictionary<string, Coroutine>();

        public void Initialize(EnemyBase logic)
        {
            Logic = logic;
            transform.position = Logic.Position;
            Logic.OnCreation();
        }
        
        public virtual void Die()
        {
            //EnemyCompendium.Instance.UnregisterEnemy(Logic);
            Logic.OnDeath();
            Destroy(gameObject); // TODO: Replace with Object Pooling
        }

        public void DealDamage(int damage)
        {
            //Debug.Log($"DEAL {damage} DMG");
            Logic.DealDamage(damage);
            Debug.Log($"HP {Logic.GetCurrentHealth()}");
            if (Logic.GetCurrentHealth() <= 0)
            {
                Die();
            }
        }

        public virtual void SetCurrentSpeed(float speed)
        {
            Logic.SetCurrentSpeed(speed);
        }

        public IEnumerator ApplyWait(float duration)
        {
            yield return new WaitForSeconds(duration);
            // Kod po oczekiwaniu
        }

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

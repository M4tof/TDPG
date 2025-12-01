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

        public void Initialize(EnemyBase logic)
        {
            Logic = logic;
            transform.position = Logic.Position;
            Logic.OnCreation();
        }

        public void ApplyWait(float duration)
        {
            StartCoroutine(WaitRoutine(duration));
        }

        private IEnumerator WaitRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

    }
}

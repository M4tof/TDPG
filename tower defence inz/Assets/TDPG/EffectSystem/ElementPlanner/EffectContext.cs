using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TDPG.EffectSystem.ElementPlanner
{
    public class EffectContext
    {
        public GameObject Attacker { get; set; }
        public GameObject Target { get; set; }
        public Vector3 HitPosition { get; set; }
        public Grid Grid { get; set; }
    }
}

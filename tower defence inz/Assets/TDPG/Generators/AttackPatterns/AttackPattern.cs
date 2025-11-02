using System;
using System.Collections.Generic;

namespace TDPG.Generators.AttackPatterns
{
    [Serializable]
    public class AttackPattern
    {
        public string id;
        public float duration;
        public List<AttackEvent> events = new List<AttackEvent>();
    }

    [Serializable]
    public struct AttackEvent
    {
        public float timeOffset;     // sec from pattern start
        public List<float> direction; // vector as list of floats (dim-dependent)
        public float speed;
        public int damage;
        public float spreadAngle;
        public string metaTag;
    }
}

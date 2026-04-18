using System.Collections.Generic;
using UnityEngine;

namespace TDPG.AudioModulation
{
        [CreateAssetMenu(fileName = "NewAudioModsList", menuName = "TDPG/Audio/Mods List")]
        public class AllowedModsListSO : ScriptableObject
        {
            [Tooltip("The list of allowed audio modifications to be chosen for this object during procedural generation")]
            public List<AudioModifier> allowedMods = new List<AudioModifier>();
        }
    
}
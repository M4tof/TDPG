using System.Collections.Generic;
using UnityEngine;

namespace TDPG.AudioModulation
{
    [CreateAssetMenu(fileName = "NewAudioModsList", menuName = "TDPG/Audio/Audio Resource List")]
    public class AudioListSO : ScriptableObject
    {
        [Tooltip("A pool of clips from which one will be selected at random (e.g., different variations of a sword swing).")]
        public List<AudioClip> audioClips = new List<AudioClip>();
    }
}
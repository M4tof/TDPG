using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    [CreateAssetMenu(fileName = "NewColorPalette", menuName = "TDPG/Color Palette")]
    public class ColorPaletteSO : ScriptableObject
    {
        [Tooltip("The list of target colors. Index 0 corresponds to Original Color 0 in the controller.")]
        public List<Color> colors = new List<Color>();
    }
}
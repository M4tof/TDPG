using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    [CreateAssetMenu(fileName = "NewAllowedPalettes", menuName = "TDPG/Allowed Palette")]
    public class AllowedPalettesSO : ScriptableObject
    {
        [Tooltip("The list of allowed palettes to be chosen for this object during procedural generation")]
        public List<ColorPaletteSO> palettes = new List<ColorPaletteSO>();
    }
}
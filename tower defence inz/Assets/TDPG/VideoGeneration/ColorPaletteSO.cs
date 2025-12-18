using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    /// <summary>
    /// Scriptable object used in the TDPG library to hold up to 16 colors in a palette.<br/>
    ///  <b>Note:</b> Additional colors will be ignored by the shader as the max of origins is 16.
    /// </summary>
    [CreateAssetMenu(fileName = "NewColorPalette", menuName = "TDPG/Color Palette")]
    public class ColorPaletteSO : ScriptableObject
    {
        [Tooltip("The list of target colors. Index 0 corresponds to Original Color 0 in the controller.")]
        public List<Color> colors = new List<Color>();
    }
}
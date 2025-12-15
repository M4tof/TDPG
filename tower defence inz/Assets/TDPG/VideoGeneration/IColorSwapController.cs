using System.Collections.Generic;
using UnityEngine;

namespace TDPG.VideoGeneration
{
    public interface IColorSwapController
    {
        /// <summary>
        /// Sets a specific single target color. 
        /// For Multi-swap controllers, this usually sets the first slot.
        /// </summary>
        void SetTargetColor(Color color);

        /// <summary>
        /// Sets the targets from a raw list of colors.
        /// </summary>
        void SetPalette(List<Color> colors);

        /// <summary>
        /// Sets the targets from a ScriptableObject palette.
        /// </summary>
        void SetPalette(ColorPaletteSO palette);

        /// <summary>
        /// Blinks quickly as white
        /// </summary>
        void BlinkWhite();
    }
}
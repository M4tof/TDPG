using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Grid;

namespace TDPG.Templates.Turret
{
    /// <summary>
    /// A visual-only representation of a turret used during the placement phase (Ghost).
    /// <br/>
    /// It inherits from <see cref="TurretBase"/> to reuse the sprite/scaling logic but adds 
    /// color tinting to indicate valid (Green) or invalid (Red) placement positions.
    /// </summary>
    public class TurretPreview : TurretBase
    {
        [Header("Preview Settings")]
        [SerializeField][Tooltip("Color tint applied when the turret CAN be placed here.")] private Color validColor = new Color(0, 1, 0, 0.5f);
        [SerializeField][Tooltip("Color tint applied when the placement is blocked.")] private Color invalidColor = new Color(1, 0, 0, 0.5f);

        /// <summary>
        /// Updates the renderer colors to reflect the current placement validity state.
        /// </summary>
        /// <param name="isValid">True = Green (Valid), False = Red (Blocked).</param>
        public void SetPlacementValid(bool isValid)
        {
            Color c = isValid ? validColor : invalidColor;

            if (baseRenderer != null) baseRenderer.color = c;
            if (crystalRenderer != null) crystalRenderer.color = c;
        }

        // We override Init to ensure alpha transparency is applied immediately
        /// <summary>
        /// Configures the ghost with the target turret's sprites and scale, 
        /// and immediately applies the default 'Valid' transparency color.
        /// </summary>
        public override void Initialize(TurretData data)
        {
            base.Initialize(data);
            SetPlacementValid(true); // Default to green
        }
    
    }
}

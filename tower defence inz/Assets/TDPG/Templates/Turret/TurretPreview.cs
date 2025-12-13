using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Grid;

namespace TDPG.Templates.Turret
{
    public class TurretPreview : TurretBase
    {
        [Header("Preview Settings")]
        [SerializeField] private Color validColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.5f);

        public void SetPlacementValid(bool isValid)
        {
            Color c = isValid ? validColor : invalidColor;

            if (baseRenderer != null) baseRenderer.color = c;
            if (crystalRenderer != null) crystalRenderer.color = c;
        }

        // We override Init to ensure alpha transparency is applied immediately
        public override void Initialize(TurretData data)
        {
            base.Initialize(data);
            SetPlacementValid(true); // Default to green
        }
    
    }
}

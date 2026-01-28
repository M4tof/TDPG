using UnityEngine;
using TDPG.Generators.AttackPatterns;

namespace TDPG.Templates.Turret
{
    [System.Serializable]
    public class CardData
    {
        public int id;
        public float damageMultiplayer = 1;
        public float fireRateMultiplayer = 1;
        public float hpMultiplayer = 1;
        
        public float rangeMultiplayer = 1;
        public int ResourceCost = 0;
        public string elementName = "";
        public AbstractAttackPatternGenerator PatternGenerator;

        public string TextInfo()
        {
            Debug.Log($"CARD {damageMultiplayer} {hpMultiplayer} {rangeMultiplayer}");
            string text = "";
            if (elementName != "")
            {
                text += "\nElement: " + elementName;
            }
            if (damageMultiplayer != 1)
            {
                text += "\nDamage: " + ToPercent(damageMultiplayer);
            }
            if (fireRateMultiplayer != 1)
            {
                text += "\nFire Rate: " + ToPercent(fireRateMultiplayer);
            }
            if (hpMultiplayer != 1)
            {
                text += "\nHp: " + ToPercent(hpMultiplayer);
            }
            if (rangeMultiplayer != 1)
            {
                text += "\nRange: " + ToPercent(rangeMultiplayer);
            }
            if (PatternGenerator != null)
            {
                text += "\nPattern: " + PatternGenerator.patternName;
            }
            if (ResourceCost > 0)
            {
                text += "\nAdditional Cost: " + ResourceCost;  
            }

            return text;
        }
        
        private string ToPercent(float value)
        {
            if (value <= 0) return "0%";
    
            if (value < 1)
            {
                int percent = Mathf.RoundToInt((1 - value) * 100);
                return $"-{percent}%";
            }
    
            // Value >= 1
            int percentIncrease = Mathf.RoundToInt((value - 1) * 100);
            return $"+{percentIncrease}%";
        }
    }
}

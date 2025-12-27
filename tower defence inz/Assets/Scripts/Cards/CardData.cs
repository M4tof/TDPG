using UnityEngine;

public class CardData
{
    public float damageMultiplayer = 1;
    public float fireRateMultiplayer = 1;
    public float hpMultiplayer = 1;
    
    public float rangeMultiplayer = 1;
    public int ResourceCost = 0;

    public string TextInfo()
    {
        string text = "";
        if (damageMultiplayer != 1)
        {
            text += "\ndamage: " + ToPercent(damageMultiplayer);
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
        text += "\nAdditional Cost: " + ResourceCost;

        return text;
    }

    private string ToPercent(float value)
    {
        if (value <= 0)
        {
            return "0";
        }
        if (value > 1)
        {
            //return "+" + ((value * 100)%100).ToString();
            return "+" + (value * 100)%100;
        }
        //return "-" + (((1 - value)*100)%100).ToString();
        return "-" + (1 - value)*100%100;
    }
    
}

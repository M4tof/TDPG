using TMPro;
using UnityEngine;

public class CardUpgrade : MonoBehaviour
{
    
    
    private CardData cardData;
    public TMP_Text text;
    private CardSelectionMenu cardSelectionMenu;

    public void SelectCard()
    {
        Debug.Log($"SELECT: {cardSelectionMenu}");
        if (cardSelectionMenu == null || cardData == null)
        {
            return;
        }

        cardSelectionMenu.SetNewCardUpgrade(cardData);
    }
    
    
    public void SetText()
    {
        text.text = cardData.TextInfo();
    }

    public void SetCardData(CardData cardData)
    {
        this.cardData = cardData;
    }

    public void SetCardSelectionMenu(CardSelectionMenu cardSelectionMenu)
    {
        this.cardSelectionMenu = cardSelectionMenu;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class CardSelectionMenu : MonoBehaviour
{
    [SerializeField] private GameObject Box;
    [SerializeField] private GameObject CardBox;
    [SerializeField] private Vector2 CardFirstPosition;
    [SerializeField] private GameObject CardUpgradePrefab;
    
    
    private int numberOfCards = 3;
    private List<CardData> cards;
    
    
    
    public void GetNewCard()
    {
        if (CardBox == null || Box == null)
        {
            return;
        }
        
        //cards = new List<CardData>();
        Box.SetActive(true);
        
        foreach (Transform child in CardBox.transform)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < numberOfCards; i++)
        {
            //Spawning card object
            Vector2 anchoredPosition  = CardFirstPosition + new Vector2(200 * i,-10);
            GameObject card = Instantiate(CardUpgradePrefab, CardBox.transform);
            RectTransform rectTransform = card.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.localScale = Vector3.one;
            }
            
            //Set Card Data Info
            CardData cardData = new CardData();
            //TODO Generowanie statystyk
            CardUpgrade cardUpgrade = card.gameObject.GetComponent<CardUpgrade>();
            if (cardUpgrade != null)
            {
                cardUpgrade.SetCardSelectionMenu(this);
                cardUpgrade.SetCardData(cardData);
                cardUpgrade.SetText();
            }
        }
        
    }

    public void SetNewCardUpgrade(CardData cardData)
    {
        Debug.Log(cardData.TextInfo());
        //TODO dodawanie karty do puli
        Box.SetActive(false);
    }
    
    public void MenuToggle()
    {
        Box.SetActive(!Box.activeSelf);
    }
}

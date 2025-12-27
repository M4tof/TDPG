using System.Collections.Generic;
using TDPG.Generators.Seed;
using TDPG.Templates.Turret;
using UnityEngine;

public class CardSelectionMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] [Tooltip("GameObject with have all visuals for selcting cards")] private GameObject SelectionBox;
    [SerializeField] [Tooltip("Object which will be parent for card buttons")] private GameObject CardBox;
    [SerializeField] [Tooltip("Start Position where would appear firs cards")] private Vector2 CardFirstPosition;
    [SerializeField] [Tooltip("Prefab for card button used to select upgrade")] private GameObject CardUpgradePrefab;
    [SerializeField] [Tooltip("Prefab for card button which show player's current cards")] private GameObject PlayerCardPrefab;
    
    [SerializeField] [Tooltip("Default number cards for selecting")] private int defautlNumberOfCards = 3;
    private List<CardData> playerCards = new List<CardData>();
    
    private int nextId = 0; //TODO WAŻNE ABY BYŁO ZAPISYWANE I WCZYTYWANE

    private TurretSelection lastUsedTurretSelection;
    
    //Show and generate new upgrades to use
    public void GetNewCard(int numberOfCards = -1)
    {
        if (numberOfCards <= 0)
        {
            numberOfCards = defautlNumberOfCards;
        }
        
        if (CardBox == null || SelectionBox == null)
        {
            return;
        }
        
        //cards = new List<CardData>();
        SelectionBox.SetActive(true);

        CleanCards();
        
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
            Debug.Log($"ITERACJA: {i}");
            CardData cardData = GenerateUpgradeData(i);
            CardUpgrade cardUpgrade = card.gameObject.GetComponent<CardUpgrade>();
            if (cardUpgrade != null)
            {
                Debug.Log(cardData.TextInfo());
                cardUpgrade.SetCardSelectionMenu(this);
                cardUpgrade.SetCardData(cardData);
                cardUpgrade.SetText();
            }
        }
    }

    public void ShowPlayersCards(TurretSelection turretSelection)
    {
        if (CardBox == null || SelectionBox == null)
        {
            return;
        }
        
        lastUsedTurretSelection = turretSelection;
        SelectionBox.SetActive(true);
        CleanCards();
        
        for (int i = 0; i < playerCards.Count; i++)
        {
            //Place Visualisation
            Vector2 anchoredPosition  = CardFirstPosition + new Vector2(200 * i,-10);
            GameObject card = Instantiate(PlayerCardPrefab, CardBox.transform);
            RectTransform rectTransform = card.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.localScale = Vector3.one;
            }
            
            //Set Card Data Info
            CardData cardData = playerCards[i];
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
        cardData.id = nextId;
        playerCards.Add(cardData);
        GenerateNextId();
        Close();
    }

    public void UseCardUpgrade(int id)
    {
        foreach (CardData playerCard in playerCards)
        {
            if (playerCard.id == id)
            {
                lastUsedTurretSelection.AddUpgrade(playerCard);
                playerCards.Remove(playerCard);
                break;
            }
        }
        Close();
    }

    public void Close()
    {
        SelectionBox.SetActive(false);
    }

    private CardData GenerateUpgradeData(int iteration)
    {
        CardData cardData = new CardData();
        
        //Seed for generating numbers
        Seed seed = GameManager.Instance.GSeed.NextSubSeed("Card Generation" + nextId * nextId + iteration);
        seed.IsBitBased = false;
        seed.NormalizeSeedValue();
        ulong seedVal = seed.GetBaseValue();
        string digits = seedVal.ToString();
        
        //Seed for generating statistics
        Seed DNA = GameManager.Instance.GSeed.NextSubSeed("Upgrade Parameters Generation" + nextId * nextId + iteration);
        DNA.IsBitBased = true;

        float ToDamage = 1;
        float ToHP = 1;
        float ToRange = 1;
        
        Debug.Log($"SEED: {digits}\n DNA: {DNA.GetBaseValue()}");
        
        if (DNA.GetBaseValue().ToString()[0] == '0')
        {
            ToDamage = Mathf.Ceil(1.01f +  int.Parse(digits.Substring(0, 2)) / 100.0f);
        }

        if (DNA.IsBitSet(0))
        {
            ToDamage = 1.01f +  int.Parse(digits.Substring(0, 2)) / 100.0f; //1.01 - 2.0
        }

        if (DNA.IsBitSet(1))
        {
            ToHP = 1.01f +  int.Parse(digits.Substring(3, 2)) / 100.0f; //1.01 - 2.0
        }
        
        if (DNA.IsBitSet(2))
        {
            ToRange = 1.01f + (int.Parse(digits.Substring(5, 2)) / 100.0f) / 4; //1.01 - 2.0
        }

        cardData.damageMultiplayer = ToDamage;
        cardData.hpMultiplayer = ToHP;
        cardData.rangeMultiplayer = ToRange;

        Debug.Log($"STATS: {ToDamage} {ToHP} {ToRange}");
        //TODO Generowanie statystyk
        return cardData;
    }

    private void CleanCards()
    {
        foreach (Transform child in CardBox.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void GenerateNextId()
    {
        nextId += 1;
    }
    
    public int GetNextId()
    {
        return nextId;
    }
    
    //Use only for loading save
    public void LoadNextId(int nextId)
    {
        this.nextId = nextId;
    }
    
}

using QuikGraph.Algorithms.Search;
using System.Collections.Generic;
using TDPG.Generators.AttackPatterns;
using TDPG.Generators.Seed;
using TDPG.Templates.Turret;
using UnityEngine;

public class CardSelectionMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField][Tooltip("GameObject with have all visuals for selcting cards")] private GameObject SelectionBox;
    [SerializeField][Tooltip("Object which will be parent for card buttons")] private GameObject CardBox;
    [SerializeField][Tooltip("Object which is parrent for turret")] private GameObject TurretBox;
    [SerializeField][Tooltip("Start Position where would appear firs cards")] private Vector2 CardFirstPosition;
    [Header("Prefabs")]
    [SerializeField][Tooltip("Prefab for card button used to select upgrade")] private GameObject CardUpgradePrefab;
    [SerializeField][Tooltip("Prefab for card button which show player's current cards")] private GameObject PlayerCardPrefab;

    [SerializeField][Tooltip("Default number cards for selecting")] private int defautlNumberOfCards = 3;


    [Header("Balancing - RNG")]
    [SerializeField] private float baseStatScale = 1.10f; // Was 1.01f (1% -> 10% base boost)
    [SerializeField] private float seedNormalizer = 100.0f; // Keep 100.0f
    [SerializeField] private float secondaryStatDivisor = 2.0f; // Was 4.0f. Secondary stats were too weak.

    [Header("References")]
    [SerializeField][Tooltip("Used to Block spawning turret while window is on")] private TurretSpawner turretSpawner;

    private void Start()
    {
        Close();
    }


    private List<CardData> playerCards = new List<CardData>();

    public int nextId = 0; //TODO WAŻNE ABY BYŁO ZAPISYWANE I WCZYTYWANE

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
        if(turretSpawner != null) { turretSpawner.SetBlockSpawnTurret(true); }
        SelectionBox.SetActive(true);

        CleanCards();

        for (int i = 0; i < numberOfCards; i++)
        {
            //Spawning card object
            Vector2 anchoredPosition = CardFirstPosition + new Vector2(200 * i, -10);
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
        if(turretSpawner != null) { turretSpawner.SetBlockSpawnTurret(true); }
        SelectionBox.SetActive(true);
        CleanCards();

        for (int i = 0; i < playerCards.Count; i++)
        {
            //Place Visualisation
            Vector2 anchoredPosition = CardFirstPosition + new Vector2(200 * i, -10);
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
                //Set Upgrade for Turret Selection
                lastUsedTurretSelection.AddUpgrade(playerCard);

                //Set Upgrade for placed turrets
                if (CardBox != null)
                {
                    for (int i = 0; i < TurretBox.transform.childCount; i++)
                    {
                        Transform child = TurretBox.transform.GetChild(i);

                        TurretBase turret = child.gameObject.GetComponent<TurretBase>();
                        if (turret == null)
                        {
                            continue;
                        }

                        if (lastUsedTurretSelection.GetTurretName() == turret.GetTurretID())
                        {
                            turret.AddAndApplyModifier(playerCard);
                            Debug.Log($"TURRET ID: {turret.GetTurretID()}");
                        }
                    }
                }

                playerCards.Remove(playerCard);
                break;
            }
        }
        Close();
    }

    public void Close()
    {
        if(turretSpawner != null) { turretSpawner.SetBlockSpawnTurret(false); }
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
        AbstractAttackPatternGenerator ToPattern = null;
        string ToElement = "";

        Debug.Log($"SEED: {digits}\n DNA: {DNA.GetBaseValue()}");

        if (DNA.GetBaseValue().ToString()[0] == '0')
        {
            ToDamage = Mathf.Ceil(baseStatScale + int.Parse(digits.Substring(0, 2)) / seedNormalizer);
        }

        if (DNA.IsBitSet(0))
        {
            ToDamage = baseStatScale + int.Parse(digits.Substring(0, 2)) / seedNormalizer; //1.01 - 2.0
        }

        if (DNA.IsBitSet(1))
        {
            ToHP = baseStatScale + int.Parse(digits.Substring(3, 2)) / seedNormalizer; //1.01 - 2.0
        }

        if (DNA.IsBitSet(2) && DNA.IsBitSet(3))
        {
            ToRange = baseStatScale + (int.Parse(digits.Substring(5, 2)) / seedNormalizer) / secondaryStatDivisor; //1.01 - 2.0
        }
        //Set New Element
        if (DNA.IsBitSet(4) && DNA.IsBitSet(5))
        {
            int elementId = int.Parse(digits.Substring(8, 1));
            string parentList = digits.Substring(9, 2);
            ToElement = RegistryManager.Instance.GetNewElementById(elementId, parentList).Name;
        }
        if (DNA.IsBitSet(6) && DNA.IsBitSet(7) && DNA.IsBitSet(8))
        {
            int patternId = int.Parse(digits.Substring(11, 1));
            patternId = patternId % 4;
            switch (patternId) {
                case 1:
                    ToPattern = new BurstAttackPatternGenerator();
                    break;
                case 2:
                    ToPattern = new BurstAttackPatternGenerator();
                    break;
                case 3:
                    ToPattern = new BurstAttackPatternGenerator();
                    break;
                default:
                    ToPattern = new BurstAttackPatternGenerator();
                    break;
            }
        }

        if (ToDamage == 1 && ToRange == 1 && ToHP == 1)
        {
            ToDamage = baseStatScale + (int.Parse(digits.Substring(7, 2)) / seedNormalizer) / secondaryStatDivisor; //1.01 - 2.0
        }

        cardData.damageMultiplayer = ToDamage;
        cardData.hpMultiplayer = ToHP;
        cardData.rangeMultiplayer = ToRange;
        cardData.elementName = ToElement;
        cardData.PatternGenerator = ToPattern;

        Debug.Log($"STATS: {ToDamage} {ToHP} {ToRange}");
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


    void OnValidate()
    {
        if (SelectionBox == null)
        {
            Debug.LogWarning("Sellection box is null", this);
        }
        if (CardBox == null)
        {
            Debug.LogWarning("Card Box is null", this);
        }
        if (TurretBox == null)
        {
            Debug.LogWarning("Turret Box is null", this);
        }
    }

}
